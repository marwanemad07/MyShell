using System.Text;
using MyShell.Core.Commands;

namespace MyShell.Core
{
    public class Shell
    {
        private readonly CommandRegistry _commandRegistry;
        private readonly ExecutableFinder _executableFinder;
        private readonly ProcessExecutor _processExecutor;

        public Shell()
        {
            _commandRegistry = new CommandRegistry();
            _executableFinder = new ExecutableFinder();
            _processExecutor = new ProcessExecutor();
            RegisterBuiltinCommands();
        }

        public void Run()
        {
            while (true)
            {
                Console.Write("$ ");
                var input = ReadLineWithAutocompletion();
                if (string.IsNullOrEmpty(input))
                    continue;
                ExecuteInput(input);
            }
        }

        private void RegisterBuiltinCommands()
        {
            _commandRegistry.RegisterCommand(new EchoCommand());
            _commandRegistry.RegisterCommand(new ExitCommand());
            _commandRegistry.RegisterCommand(new TypeCommand(_commandRegistry));
            _commandRegistry.RegisterCommand(new PwdCommand());
            _commandRegistry.RegisterCommand(new CdCommand());
            _commandRegistry.RegisterCommand(new HistoryCommand());
        }

        private void ExecuteInput(string input)
        {
            var inputs = Parser.ParseInput(input);

            if (inputs.Count > 1)
            {
                ExecutePipeline(inputs);
            }
            else
            {
                var (command, args) = inputs[0];
                ExecuteCommand(command, args);
            }
        }

        private void ExecutePipeline(List<(string command, List<string> args)> commands)
        {
            // Validate all commands exist before executing
            for (int i = 0; i < commands.Count; i++)
            {
                var (cmd, _) = commands[i];
                var builtin = _commandRegistry.Get(cmd);
                var external = _executableFinder.FindExecutable(cmd);

                if (builtin == null && external == null)
                {
                    Console.WriteLine($"{cmd}: command not found");
                    return;
                }
            }

            // Execute the pipeline
            _processExecutor.ExecutePipeline(commands, _commandRegistry);
        }

        private void ExecuteCommand(string command, List<string> args)
        {
            var commandInstance = _commandRegistry.Get(command);

            if (commandInstance == null)
            {
                if (_executableFinder.FindExecutable(command) != null)
                {
                    _processExecutor.Execute(command, args);
                    return;
                }
                Console.WriteLine($"{command}: command not found");
                return;
            }
            commandInstance.Execute(args);
        }

        // TODO: This method can be moved to Helper class
        // TODO: We will use trie data structure for better performance
        private string? ReadLineWithAutocompletion()
        {
            var currentLineBuffer = new System.Text.StringBuilder();
            int cursorPosition = 0;
            bool tabPressedOnce = true;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return currentLineBuffer.ToString();
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    HandleTabKey(currentLineBuffer, ref cursorPosition, ref tabPressedOnce);
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    HandleBackspaceKey(currentLineBuffer, ref cursorPosition);
                    tabPressedOnce = true;
                }
                else
                {
                    HandleCharacterInput(currentLineBuffer, ref cursorPosition, key.KeyChar);
                    tabPressedOnce = true;
                }
            }
        }

        private void HandleTabKey(
            StringBuilder buffer,
            ref int cursorPosition,
            ref bool tabPressedOnce
        )
        {
            var currentInput = buffer.ToString();
            var completions = TryAutocomplete(currentInput);

            if (completions != null)
            {
                if (completions.Count == 1)
                {
                    // TODO: here we can hanlde it by adding the remaining part only that differs from current input
                    ReplaceLineContent(buffer, completions[0] + " ", ref cursorPosition);
                    tabPressedOnce = true;
                }
                else if (completions.Count > 1)
                {
                    var lcp = StringUtilities.GetLongestCommonPrefix(completions);
                    if (lcp.Length > currentInput.Length)
                    {
                        // TODO: here we can hanlde it by adding the remaining part only that differs from current input
                        ReplaceLineContent(buffer, lcp, ref cursorPosition);
                        tabPressedOnce = true;
                    }
                    else if (tabPressedOnce)
                    {
                        tabPressedOnce = false;
                        Console.Write("\a");
                    }
                    else
                    {
                        Console.WriteLine($"\n{string.Join("  ", completions)}");
                        Console.Write("\r$ " + buffer.ToString());
                        tabPressedOnce = true;
                    }
                }
            }
            else
            {
                Console.Write("\a");
                tabPressedOnce = true;
            }
        }

        private void HandleBackspaceKey(StringBuilder buffer, ref int cursorPosition)
        {
            if (cursorPosition > 0)
            {
                buffer.Remove(cursorPosition - 1, 1);
                cursorPosition--;
                // move cursor back, overwrite the character, and move cursor back again
                Console.Write("\b \b");
            }
        }

        private void HandleCharacterInput(
            StringBuilder buffer,
            ref int cursorPosition,
            char keyChar
        )
        {
            buffer.Append(keyChar);
            cursorPosition++;
            Console.Write(keyChar);
        }

        private void ReplaceLineContent(
            StringBuilder buffer,
            string newContent,
            ref int cursorPosition
        )
        {
            Console.Write("\r$ " + new string(' ', buffer.Length));
            buffer.Clear();
            buffer.Append(newContent);
            cursorPosition = buffer.Length;
            Console.Write("\r$ " + buffer.ToString());
        }

        private List<string>? TryAutocomplete(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (input.Contains(' '))
                return null;

            var builtinMatches = _commandRegistry
                .GetBuiltinCommands()
                .Where(cmd => cmd.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var executableMatches = _executableFinder.GetExecutablesStartingWith(input);

            var allMatches = builtinMatches
                .Concat(executableMatches)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return allMatches.Count > 0 ? allMatches : null;
        }
    }
}
