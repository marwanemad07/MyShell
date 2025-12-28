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
    }
}
