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

            if (Directory.Exists(args[0]))
            {
                Directory.SetCurrentDirectory(args[0]);
            }
            else
            {
                Console.WriteLine($"cd: {args[0]}: No such file or directory");
            }

            return 0;
        }
    }
}
