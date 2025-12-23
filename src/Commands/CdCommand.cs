namespace MyShell.Core.Commands
{
    public class CdCommand : ICommand
    {
        public string Name => "cd";

        public int Execute(List<string> args)
        {
            if (args == null || args.Count == 0)
            {
                Console.WriteLine("Usage: cd <directory>");
                return 1;
            }

            bool isRooted = Path.IsPathRooted(args[0]);

            if (isRooted)
            {
                ChangeToAbsolutePath(args[0]);
            }
            else
            {
                var newPath = Path.Combine(Environment.CurrentDirectory, args[0]);
                ChangeToAbsolutePath(newPath);
            }

            return 0;
        }

        private void ChangeToAbsolutePath(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.SetCurrentDirectory(path);
            }
            else
            {
                Console.WriteLine($"cd: {path}: No such file or directory");
            }
        }
    }
}
