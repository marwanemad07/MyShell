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

        public void ExecuteExternalToExternal(
            string command1,
            List<string> args1,
            string command2,
            List<string> args2
        )
        {
            try
            {
                var redirectionOptions1 = RedirectionOptions.Parse(args1);
                var redirectionOptions2 = RedirectionOptions.Parse(args2);

                var process1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command1,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                    },
                };

                var process2 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command2,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                    },
                };

                var filteredArgs1 = redirectionOptions1.GetFilteredArgs(args1);
                foreach (var arg in filteredArgs1)
                {
                    process1.StartInfo.ArgumentList.Add(arg);
                }

                var filteredArgs2 = redirectionOptions2.GetFilteredArgs(args2);
                foreach (var arg in filteredArgs2)
                {
                    process2.StartInfo.ArgumentList.Add(arg);
                }

                var redirectionHandler2 = new RedirectionHandler(redirectionOptions2);
                redirectionHandler2.AttachToProcess(process2);

                process1.Start();
                process2.Start();

                process2.BeginOutputReadLine();
                process2.BeginErrorReadLine();

                // pipe stdout of process1 to stdin of process2
                var copyTask = process1.StandardOutput.BaseStream.CopyToAsync(
                    process2.StandardInput.BaseStream
                );

                process1.StandardInput.Close();

                // handle stderr from process1
                Task.Run(() =>
                {
                    string? line;
                    while ((line = process1.StandardError.ReadLine()) != null)
                    {
                        Console.Error.WriteLine(line);
                    }
                });

                // wait for process1 to finish
                process1.WaitForExit();
                copyTask.Wait(); // ensure all data is copied

                // close stdin of process2
                process2.StandardInput.Close();

                // wait for process2 to finish
                process2.WaitForExit();

                // handle any redirected output from process2
                redirectionHandler2.WriteRedirectedOutput();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing pipeline: {ex.Message}");
            }
        }

        public void ExecuteBuiltinToExternal(
            Commands.ICommand builtin,
            List<string> args1,
            string command2,
            List<string> args2
        )
        {
            try
            {
                var redirectionOptions2 = RedirectionOptions.Parse(args2);

                // capture output from built-in command
                var outputWriter = new StringWriter();
                var originalOut = Console.Out;
                Console.SetOut(outputWriter);

                builtin.Execute(args1);

                Console.SetOut(originalOut);
                var builtinOutput = outputWriter.ToString();

                // create process for second command
                var process2 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command2,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                    },
                };

                var filteredArgs2 = redirectionOptions2.GetFilteredArgs(args2);
                foreach (var arg in filteredArgs2)
                {
                    process2.StartInfo.ArgumentList.Add(arg);
                }

                var redirectionHandler2 = new RedirectionHandler(redirectionOptions2);
                redirectionHandler2.AttachToProcess(process2);

                process2.Start();
                process2.BeginOutputReadLine();
                process2.BeginErrorReadLine();

                // write built-in output to process2's stdin
                var writer = process2.StandardInput;
                writer.Write(builtinOutput);
                writer.Close();

                process2.WaitForExit();
                redirectionHandler2.WriteRedirectedOutput();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing pipeline: {ex.Message}");
            }
        }

        public void ExecuteExternalToBuiltin(
            string command1,
            List<string> args1,
            Commands.ICommand builtin,
            List<string> args2
        )
        {
            try
            {
                var redirectionOptions1 = RedirectionOptions.Parse(args1);

                // create process for first command
                var process1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command1,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                    },
                };

                var filteredArgs1 = redirectionOptions1.GetFilteredArgs(args1);
                foreach (var arg in filteredArgs1)
                {
                    process1.StartInfo.ArgumentList.Add(arg);
                }

                process1.Start();
                process1.StandardInput.Close();

                // capture output from process1
                var output = process1.StandardOutput.ReadToEnd();
                var errorOutput = process1.StandardError.ReadToEnd();

                process1.WaitForExit();

                if (!string.IsNullOrEmpty(errorOutput))
                {
                    Console.Error.Write(errorOutput);
                }

                // redirect stdin for built-in command
                var originalIn = Console.In;
                Console.SetIn(new StringReader(output));

                builtin.Execute(args2);

                Console.SetIn(originalIn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing pipeline: {ex.Message}");
            }
        }

        public void ExecuteBuiltinToBuiltin(
            Commands.ICommand builtin1,
            List<string> args1,
            Commands.ICommand builtin2,
            List<string> args2
        )
        {
            try
            {
                // capture output from first built-in
                var outputWriter = new StringWriter();
                var originalOut = Console.Out;
                Console.SetOut(outputWriter);

                builtin1.Execute(args1);

                Console.SetOut(originalOut);
                var output = outputWriter.ToString();

                // redirect stdin for second built-in
                var originalIn = Console.In;
                Console.SetIn(new StringReader(output));

                builtin2.Execute(args2);

                Console.SetIn(originalIn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing pipeline: {ex.Message}");
            }
        }
    }
}
