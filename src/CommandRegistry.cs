using MyShell.Core.Commands;

namespace MyShell.Core
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands = new();

        public void RegisterCommand(ICommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("Command name cannot be empty.");

            if (_commands.ContainsKey(command.Name))
                throw new InvalidOperationException(
                    $"Command '{command.Name}' is already registered.");

            _commands.Add(command.Name, command);
        }

        public ICommand? Get(string name)
        {
            _commands.TryGetValue(name, out var command);
            return command;
        }
    }
}
