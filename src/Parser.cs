namespace MyShell.Core
{
    public static class Parser
    {
        public static List<(string command, List<string> args)> ParseInput(string input)
        {
            // TODO: this is a simplified parser; it does not handle all edge cases
            // such as escaped pipes or quotes within commands.
            var segments = input.Split('|');

            var result = new List<(string, List<string>)>();

            foreach (var segment in segments)
            {
                var args = ProcessInput(segment);
                var command = args.Count > 0 ? args[0] : string.Empty;
                var commandArgs = args.Count > 1 ? args.Skip(1).ToList() : [];
                result.Add((command, commandArgs));
            }
            return result;
        }

        private static List<string> ProcessInput(string input)
        {
            input = input.Trim();
            var inSingleQuotes = false;
            var inDoubleQuotes = false;
            var escaped = false;
            var currentArg = string.Empty;
            var argList = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                var ch = input[i];

                if (escaped)
                {
                    currentArg += ch;
                    escaped = false;
                    continue;
                }

                // handle double quotes
                if (ch == '\"')
                {
                    if (inSingleQuotes)
                    {
                        currentArg += ch;
                        continue;
                    }
                    inDoubleQuotes = !inDoubleQuotes;
                    continue;
                }

                // handle single quotes
                if (ch == '\'')
                {
                    if (inDoubleQuotes)
                    {
                        currentArg += ch;
                        continue;
                    }
                    inSingleQuotes = !inSingleQuotes;
                    continue;
                }

                if (ch == '\\')
                {
                    if (inSingleQuotes)
                    {
                        currentArg += ch;
                    }
                    else if (inDoubleQuotes)
                    {
                        // In double quotes, only certain characters can be escaped
                        if (i + 1 < input.Length && (input[i + 1] == '\"' || input[i + 1] == '\\'))
                        {
                            escaped = true;
                        }
                        else
                        {
                            currentArg += ch;
                        }
                    }
                    else
                    {
                        escaped = true;
                    }
                    continue;
                }

                // handle spaces
                if (ch == ' ' && !inSingleQuotes && !inDoubleQuotes)
                {
                    if (!string.IsNullOrEmpty(currentArg))
                    {
                        argList.Add(currentArg);
                        currentArg = string.Empty;
                    }
                }
                else
                {
                    currentArg += ch;
                }
            }

            if (!inSingleQuotes && !inDoubleQuotes && !string.IsNullOrEmpty(currentArg))
            {
                argList.Add(currentArg);
            }

            // TODO: Handle unclosed quotes if necessary

            return argList;
        }
    }
}
