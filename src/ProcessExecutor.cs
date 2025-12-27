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
    }
}
