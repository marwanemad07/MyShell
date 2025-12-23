using System.Runtime.InteropServices;

namespace MyShell.Core
{
    public static class Helper
    {
        public static bool IsExecutable(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Fuck windows
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