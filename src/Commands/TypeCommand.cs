namespace MyShell.Core.Commands
{
    public class TypeCommand : ICommand
    {
        private readonly CommandRegistry _commandRegistry;
        private readonly ExecutableFinder _executableFinder;

        public TypeCommand(CommandRegistry commandRegistry)
        {
            _commandRegistry = commandRegistry;
            _executableFinder = new ExecutableFinder();
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

            var filePath = _executableFinder.FindExecutable(args[0]);
            if (filePath != null)
            {
                Console.WriteLine($"{args[0]} is {filePath}");
                return 0;
            }

            Console.WriteLine($"{args[0]}: not found");
            return 0;
        }
    }
}
