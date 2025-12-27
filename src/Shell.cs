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
            _commandRegistry.RegisterCommand(new PwdCommand());
            _commandRegistry.RegisterCommand(new CdCommand());
        }

        public void Run()
        {
            while (true)
            {
                Console.Write("$ ");

                var input = ReadLineWithAutocompletion();

                var (command, args) = Parser.ParseInput(input ?? string.Empty);

                if (string.IsNullOrEmpty(input))
                    continue;

                var commandInstance = _commandRegistry.Get(command);

                if (commandInstance == null)
                {
                    if (Helper.CheckExecutableFileExists(command) != null)
                    {
                        Helper.ExecuteExternalProgram(command, args);
                        continue;
                    }

                    Console.WriteLine($"{command}: command not found");
                    continue;
                }

                commandInstance.Execute(args);
            }
        }

        private string? ReadLineWithAutocompletion()
        {
            var input = new System.Text.StringBuilder();
            int cursorPosition = 0;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    var currentInput = input.ToString();
                    var completions = TryAutocomplete(currentInput);

                    if (completions != null)
                    {
                        if (completions.Count == 1)
                        {
                            // clear current line
                            Console.Write("\r$ " + new string(' ', input.Length));

                            // write the completed command
                            input.Clear();
                            input.Append(completions[0]);
                            input.Append(' ');
                            cursorPosition = input.Length;

                            Console.Write("\r$ " + input.ToString());
                        }
                        else if (completions.Count > 1)
                        {
                            var lcp = Helper.GetLongestCommonPrefix(completions);
                            // clear current line
                            Console.Write("\r$ " + new string(' ', input.Length));

                            // write the longest common prefix
                            input.Clear();
                            input.Append(lcp);
                            cursorPosition = input.Length;

                            Console.Write("\r$ " + input.ToString());
                        }
                        else
                        {
                            // trying to solve a bug where no completions are found
                            Console.WriteLine($"belling for no completions");
                            Console.Write("\a");
                        }
                    }
                    else
                    {
                        // no completion found, do a bell sound
                        Console.WriteLine($"belling for no completions");
                        Console.Write("\a");
                    }
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (cursorPosition > 0)
                    {
                        input.Remove(cursorPosition - 1, 1);
                        cursorPosition--;

                        // redraw the line
                        Console.Write("\r$ " + input.ToString() + " ");
                        Console.Write("\r$ " + input.ToString());

                        // position cursor correctly
                        if (cursorPosition < input.Length)
                        {
                            Console.Write("\r$ ");
                            for (int i = 0; i < cursorPosition; i++)
                            {
                                Console.Write(input[i]);
                            }
                        }
                    }
                }
                else
                {
                    input.Insert(cursorPosition, key.KeyChar);
                    cursorPosition++;

                    // redraw the line
                    Console.Write("\r$ " + input.ToString());

                    // position cursor correctly
                    if (cursorPosition < input.Length)
                    {
                        Console.Write("\r$ ");
                        for (int i = 0; i < cursorPosition; i++)
                        {
                            Console.Write(input[i]);
                        }
                    }
                }
            }
        }

        private List<string>? TryAutocomplete(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // autocomplete if there are no spaces (we're still on the first word)
            if (input.Contains(' '))
                return null;

            // builtin commands that match
            var builtinMatches = _commandRegistry
                .GetBuiltinCommands()
                .Where(cmd => cmd.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // external executables that match
            var executableMatches = Helper.GetExecutablesStartingWith(input);

            // Combine all matches
            var allMatches = builtinMatches
                .Concat(executableMatches)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return allMatches.Count > 0 ? allMatches : null;
        }
    }
}
