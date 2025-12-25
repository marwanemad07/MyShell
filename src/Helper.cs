using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MyShell.Core
{
    public static class Helper
    {
        public static string? CheckExecutableFileExists(string filename)
        {
            var paths = Environment.GetEnvironmentVariable("PATH")!.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, filename);
                if (IsExecutable(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }

        public static void ExecuteExternalProgram(string command, List<string> args)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;

                bool outputRedirection = IsOutputRedirection(args);
                bool errorRedirection = IsErrorRedirection(args);
                bool appendOutputRedirection = IsAppednOutputRedirection(args);
                bool appendErrorRedirection = IsAppendErrorRedirection(args);

                if (
                    outputRedirection
                    || errorRedirection
                    || appendOutputRedirection
                    || appendErrorRedirection
                )
                {
                    // remove redirection tokens from args
                    var filteredArgs = args.Take(args.Count - 2).ToList();
                    foreach (var arg in filteredArgs)
                        process.StartInfo.ArgumentList.Add(arg);
                }
                else
                {
                    foreach (var arg in args)
                        process.StartInfo.ArgumentList.Add(arg);
                }

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        if (outputRedirection)
                            outputBuilder.AppendLine(e.Data);
                        else if (appendOutputRedirection)
                            outputBuilder.AppendLine(e.Data);
                        else
                            Console.WriteLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        if (errorRedirection)
                            errorBuilder.AppendLine(e.Data);
                        else if (appendErrorRedirection)
                            errorBuilder.AppendLine(e.Data);
                        else
                            Console.Error.WriteLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (outputRedirection)
                    WriteToFile(outputBuilder.ToString().TrimEnd(), args[^1]);

                if (appendOutputRedirection)
                    WriteToFile(outputBuilder.ToString().TrimEnd(), args[^1], append: true);

                if (errorRedirection)
                    WriteToFile(errorBuilder.ToString().TrimEnd(), args[^1]);

                if (appendErrorRedirection)
                    WriteToFile(errorBuilder.ToString().TrimEnd(), args[^1], append: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            }
        }

        public static void WriteToFile(string? output, string filePath, bool append = false)
        {
            try
            {
                using var writer = new StreamWriter(filePath, append);
                if (output != null && output != "")
                    writer.WriteLine(output);
                else
                    writer.Write("");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file '{filePath}': {ex.Message}");
            }
        }

        public static bool IsOutputRedirection(List<string> args)
        {
            return args.Contains("1>") || args.Contains(">");
        }

        public static bool IsErrorRedirection(List<string> args)
        {
            return args.Contains("2>");
        }

        public static bool IsAppednOutputRedirection(List<string> args)
        {
            return args.Contains("1>>") || args.Contains(">>");
        }

        public static bool IsAppendErrorRedirection(List<string> args)
        {
            return args.Contains("2>>");
        }

        private static bool IsExecutable(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (File.Exists(path))
                    return IsWindowsExecutableExtension(path);

                // if no extension, try PATHEXT expansion (bash -> bash.exe)
                if (!Path.HasExtension(path))
                {
                    string pathext = Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE";
                    foreach (var ext in pathext.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string candidate = path + ext;
                        if (File.Exists(candidate))
                            return true;
                    }
                }

                return false;
            }
            else
            {
                if (!File.Exists(path))
                    return false;

                var mode = File.GetUnixFileMode(path);
                return mode.HasFlag(UnixFileMode.UserExecute)
                    || mode.HasFlag(UnixFileMode.GroupExecute)
                    || mode.HasFlag(UnixFileMode.OtherExecute);
            }
        }

        private static bool IsWindowsExecutableExtension(string path)
        {
            string ext = Path.GetExtension(path);
            string pathext = Environment.GetEnvironmentVariable("PATHEXT") ?? "";

            return pathext
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}
