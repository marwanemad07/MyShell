using System.Diagnostics;
using System.Runtime.InteropServices;

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
                foreach (var arg in args)
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        Console.WriteLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        Console.Error.WriteLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            }
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
