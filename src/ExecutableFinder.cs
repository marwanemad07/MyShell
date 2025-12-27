using System.Runtime.InteropServices;

namespace MyShell.Core
{
    public class ExecutableFinder
    {
        public string? FindExecutable(string filename)
        {
            var pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVariable))
                return null;

            var paths = pathVariable.Split(Path.PathSeparator);
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

        public List<string> GetExecutablesStartingWith(string prefix)
        {
            var executables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var pathVariable = Environment.GetEnvironmentVariable("PATH");

            if (string.IsNullOrEmpty(pathVariable))
                return new List<string>();

            var paths = pathVariable.Split(
                Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var path in paths)
            {
                AddExecutablesFromDirectory(path, prefix, executables);
            }

            return executables.ToList();
        }

        private void AddExecutablesFromDirectory(
            string directoryPath,
            string prefix,
            HashSet<string> executables
        )
        {
            if (!Directory.Exists(directoryPath))
                return;

            try
            {
                var files = Directory.GetFiles(directoryPath);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (
                        fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        && IsExecutable(file)
                    )
                    {
                        executables.Add(fileName);
                    }
                }
            }
            catch
            {
                // Ignore directories we can't access
            }
        }

        private bool IsExecutable(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return IsWindowsExecutable(path);
            }
            else
            {
                return IsUnixExecutable(path);
            }
        }

        private bool IsWindowsExecutable(string path)
        {
            if (File.Exists(path))
                return HasWindowsExecutableExtension(path);

            // If no extension, try PATHEXT expansion (e.g., bash -> bash.exe)
            if (!Path.HasExtension(path))
            {
                return TryFindWithPathExtensions(path);
            }

            return false;
        }

        private bool TryFindWithPathExtensions(string path)
        {
            var pathExt = Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE";
            var extensions = pathExt.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var ext in extensions)
            {
                var candidate = path + ext;
                if (File.Exists(candidate))
                    return true;
            }

            return false;
        }

        private bool HasWindowsExecutableExtension(string path)
        {
            var extension = Path.GetExtension(path);
            var pathExt = Environment.GetEnvironmentVariable("PATHEXT") ?? "";

            return pathExt
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsUnixExecutable(string path)
        {
            if (!File.Exists(path))
                return false;

            if (!OperatingSystem.IsWindows())
            {
                var mode = File.GetUnixFileMode(path);
                return mode.HasFlag(UnixFileMode.UserExecute)
                    || mode.HasFlag(UnixFileMode.GroupExecute)
                    || mode.HasFlag(UnixFileMode.OtherExecute);
            }

            return false;
        }
    }
}
