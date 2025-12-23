using System.Runtime.InteropServices;

namespace MyShell.Core.Commands
{
    public class TypeCommand : ICommand
    {
        private readonly CommandRegistry _commandRegistry;

        public TypeCommand(CommandRegistry commandRegistry)
        {
            _commandRegistry = commandRegistry;
        }

        public string Name => "type";

        public int Execute(List<string> args)
        {
            if (args == null || args.Count == 0)
            {
                Console.WriteLine("Usage: type <filename>");
                return 1;
            }

            var command = _commandRegistry.Get(args[0]);
            if (command != null)
            {
                Console.WriteLine($"{command.Name} is a shell builtin");
                return 0;
            }

            var filePath = CheckFileExecutableExists(args[0]);
            if (filePath != null)
            {
                Console.WriteLine($"{args[0]} is {filePath}");
                return 0;
            }

            Console.WriteLine($"{args[0]}: not found");
            return 0;
        }

        private string? CheckFileExecutableExists(string filename)
        {
            Console.WriteLine($"Checking if {filename} exists in PATH directories...");
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

        public bool IsExecutable(string path)
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
                    Console.WriteLine($"Checking PATHEXT expansions for {path} with PATHEXT={pathext}");
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

        private bool IsWindowsExecutableExtension(string path)
        {
            string ext = Path.GetExtension(path);
            string pathext = Environment.GetEnvironmentVariable("PATHEXT") ?? "";
            Console.WriteLine($"Checking if extension {ext} is in PATHEXT {pathext}...");

            return pathext
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}
