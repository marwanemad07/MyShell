using MyShell.Core.Commands;

namespace MyShell.Core
{
    public class Shell
    {
        private readonly CommandRegistry _commandRegistry;

        public Shell()
        {
            _commandRegistry = new CommandRegistry();
            _commandRegistry.RegisterCommand(new EchoCommand());
            _commandRegistry.RegisterCommand(new ExitCommand());
            _commandRegistry.RegisterCommand(new TypeCommand(_commandRegistry));
        }

        public void Run()
        {
            while (true)
            {
                Console.Write("$ ");

                var input = Console.ReadLine()?.Trim();

                var (command, args) = Parser.ParseInput(input ?? string.Empty);

                if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                    break;

                var commandInstance = _commandRegistry.Get(command);

                if (commandInstance == null)
                {
                    Console.WriteLine($"{command}: command not found");
                    continue;
                }

                commandInstance.Execute(args);
            }
        }
    }
}
