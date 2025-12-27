using MyShell.Core.Commands;

namespace MyShell.Core
{
    public class Shell
    {
        private readonly CommandRegistry _commandRegistry;

        public Shell()
        {
            _commandRegistry = new CommandRegistry();
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
        }

        private void ExecuteInput(string input)
        {
            var (command, args) = Parser.ParseInput(input);
            var commandInstance = _commandRegistry.Get(command);

            if (commandInstance == null)
            {
                if (Helper.CheckExecutableFileExists(command) != null)
                {
                    Helper.ExecuteExternalProgram(command, args);
                    return;
                }
                Console.WriteLine($"{command}: command not found");
                return;
            }
            commandInstance.Execute(args);
        }

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
                    RedrawLine(buffer, completions[0] + " ", ref cursorPosition);
                    tabPressedOnce = true;
                }
                else if (completions.Count > 1)
                {
                    var lcp = Helper.GetLongestCommonPrefix(completions);
                    if (lcp.Length > currentInput.Length)
                    {
                        RedrawLine(buffer, lcp, ref cursorPosition);
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

        private void HandleBackspaceKey(System.Text.StringBuilder buffer, ref int cursorPosition)
        {
            if (cursorPosition > 0)
            {
                buffer.Remove(cursorPosition - 1, 1);
                cursorPosition--;
                RedrawLine(buffer, buffer.ToString(), ref cursorPosition);
            }
        }

        private void HandleCharacterInput(
            System.Text.StringBuilder buffer,
            ref int cursorPosition,
            char keyChar
        )
        {
            buffer.Insert(cursorPosition, keyChar);
            cursorPosition++;
            RedrawLine(buffer, buffer.ToString(), ref cursorPosition);
        }

        private void RedrawLine(
            System.Text.StringBuilder buffer,
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

            var executableMatches = Helper.GetExecutablesStartingWith(input);

            var allMatches = builtinMatches
                .Concat(executableMatches)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return allMatches.Count > 0 ? allMatches : null;
        }
    }
}
