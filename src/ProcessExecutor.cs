using System.Diagnostics;

namespace MyShell.Core
{
    public class ProcessExecutor
    {
        public void Execute(string command, List<string> args)
        {
            try
            {
                var redirectionOptions = RedirectionOptions.Parse(args);
                var process = CreateProcess(command, args, redirectionOptions);
                var redirectionHandler = new RedirectionHandler(redirectionOptions);

                redirectionHandler.AttachToProcess(process);
                RunProcess(process);
                redirectionHandler.WriteRedirectedOutput();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            }
        }

        private Process CreateProcess(
            string command,
            List<string> args,
            RedirectionOptions redirectionOptions
        )
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
            };

            var filteredArgs = redirectionOptions.GetFilteredArgs(args);
            foreach (var arg in filteredArgs)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            return process;
        }

        private void RunProcess(Process process)
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        public void ExecutePipeline(
            List<(string command, List<string> args)> commands,
            CommandRegistry commandRegistry
        )
        {
            try
            {
                string? inputForNext = null;

                for (int i = 0; i < commands.Count; i++)
                {
                    var (cmd, args) = commands[i];
                    var builtin = commandRegistry.Get(cmd);
                    var isLastCommand = i == commands.Count - 1;

                    if (builtin != null)
                    {
                        inputForNext = ExecuteBuiltinInPipeline(
                            builtin,
                            args,
                            inputForNext,
                            isLastCommand
                        );
                    }
                    else
                    {
                        inputForNext = ExecuteExternalInPipeline(
                            cmd,
                            args,
                            inputForNext,
                            isLastCommand
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing pipeline: {ex.Message}");
            }
        }

        private string? ExecuteBuiltinInPipeline(
            Commands.ICommand builtin,
            List<string> args,
            string? input,
            bool isLastCommand
        )
        {
            TextReader? originalIn = null;
            if (input != null)
            {
                originalIn = Console.In;
                Console.SetIn(new StringReader(input));
            }

            if (isLastCommand)
            {
                builtin.Execute(args);

                if (originalIn != null)
                {
                    Console.SetIn(originalIn);
                }
                return null;
            }
            else
            {
                var outputWriter = new StringWriter();
                var originalOut = Console.Out;
                Console.SetOut(outputWriter);

                builtin.Execute(args);

                Console.SetOut(originalOut);
                if (originalIn != null)
                {
                    Console.SetIn(originalIn);
                }

                return outputWriter.ToString();
            }
        }

        private string? ExecuteExternalInPipeline(
            string command,
            List<string> args,
            string? input,
            bool isLastCommand
        )
        {
            var redirectionOptions = RedirectionOptions.Parse(args);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                },
            };

            var filteredArgs = redirectionOptions.GetFilteredArgs(args);
            foreach (var arg in filteredArgs)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            if (isLastCommand)
            {
                var redirectionHandler = new RedirectionHandler(redirectionOptions);
                redirectionHandler.AttachToProcess(process);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // write input to process if we have it
                if (input != null)
                {
                    process.StandardInput.Write(input);
                }
                process.StandardInput.Close();

                process.WaitForExit();
                redirectionHandler.WriteRedirectedOutput();

                return null;
            }
            else
            {
                process.Start();

                // write input to process if we have it
                if (input != null)
                {
                    process.StandardInput.Write(input);
                }
                process.StandardInput.Close();

                // capture output
                var output = process.StandardOutput.ReadToEnd();
                var errorOutput = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(errorOutput))
                {
                    Console.Error.Write(errorOutput);
                }

                return output;
            }
        }
    }
}
