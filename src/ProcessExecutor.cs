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
                var segments = new List<List<(string cmd, List<string> args, bool isBuiltin)>>();
                var currentSegment = new List<(string, List<string>, bool)>();

                for (int i = 0; i < commands.Count; i++)
                {
                    var (cmd, args) = commands[i];
                    var builtin = commandRegistry.Get(cmd);

                    if (builtin != null)
                    {
                        if (currentSegment.Count > 0)
                        {
                            segments.Add(currentSegment);
                            currentSegment = new();
                        }
                        // add a segment containing only the builtin command
                        segments.Add(new List<(string, List<string>, bool)> { (cmd, args, true) });
                    }
                    else
                    {
                        currentSegment.Add((cmd, args, false));
                    }
                }

                if (currentSegment.Count > 0)
                {
                    segments.Add(currentSegment);
                }

                string? inputForNextSegment = null;
                for (int i = 0; i < segments.Count; i++)
                {
                    var segment = segments[i];
                    var isLastSegment = i == segments.Count - 1;

                    if (segment.Count == 1 && segment[0].isBuiltin)
                    {
                        var builtin = commandRegistry.Get(segment[0].cmd);
                        inputForNextSegment = ExecuteBuiltinInPipeline(
                            builtin!, // sure from above check
                            segment[0].args,
                            inputForNextSegment,
                            isLastSegment
                        );
                    }
                    else
                    {
                        inputForNextSegment = ExecuteExternalSegment(
                            segment,
                            inputForNextSegment,
                            isLastSegment
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing pipeline: {ex.Message}");
            }
        }

        private string? ExecuteExternalSegment(
            List<(string cmd, List<string> args, bool isBuiltin)> segment,
            string? input,
            bool isLastSegment
        )
        {
            var processes = new List<Process>();
            var redirectionOptions = segment.Select(s => RedirectionOptions.Parse(s.args)).ToList();

            for (int i = 0; i < segment.Count; i++)
            {
                var (cmd, args, _) = segment[i];
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = cmd,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                    },
                };

                var filteredArgs = redirectionOptions[i].GetFilteredArgs(args);
                foreach (var arg in filteredArgs)
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }

                processes.Add(process);
            }

            var lastProcess = processes[^1];
            RedirectionHandler? redirectionHandler = null;

            if (isLastSegment)
            {
                redirectionHandler = new RedirectionHandler(redirectionOptions[^1]);
                redirectionHandler.AttachToProcess(lastProcess);
            }

            foreach (var process in processes)
            {
                process.Start();
            }

            if (isLastSegment)
            {
                lastProcess.BeginOutputReadLine();
                lastProcess.BeginErrorReadLine();
            }

            // connect the pipeline, copy outputs to inputs
            var copyTasks = new List<Task>();

            for (int i = 0; i < processes.Count - 1; i++)
            {
                var currentProcess = processes[i];
                var nextProcess = processes[i + 1];

                var copyTask = currentProcess.StandardOutput.BaseStream.CopyToAsync(
                    nextProcess.StandardInput.BaseStream
                );
                copyTasks.Add(copyTask);

                var processIndex = i;
                Task.Run(() =>
                {
                    string? line;
                    while ((line = currentProcess.StandardError.ReadLine()) != null)
                    {
                        Console.Error.WriteLine(line);
                    }
                });
            }

            if (input != null)
            {
                processes[0].StandardInput.Write(input);
            }
            processes[0].StandardInput.Close();

            for (int i = 0; i < processes.Count - 1; i++)
            {
                processes[i].WaitForExit();
                copyTasks[i].Wait();
                processes[i + 1].StandardInput.Close();
            }

            if (isLastSegment)
            {
                lastProcess.WaitForExit();
                redirectionHandler?.WriteRedirectedOutput();
                return null;
            }
            else
            {
                var output = lastProcess.StandardOutput.ReadToEnd();
                var errorOutput = lastProcess.StandardError.ReadToEnd();

                lastProcess.WaitForExit();

                if (!string.IsNullOrEmpty(errorOutput))
                {
                    Console.Error.Write(errorOutput);
                }

                return output;
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
    }
}
