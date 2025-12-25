namespace MyShell.Core
{
    public static class Parser
    {
        public static (string command, List<string> args) ParseInput(string input)
        {
            var processedInput = ProcessInput(input);

            var command = processedInput.Count > 0 ? processedInput[0] : string.Empty;
            var argsList = processedInput.Count > 1 ? processedInput.Skip(1).ToList() : [];

            return (command, argsList);
        }

        private static List<string> ProcessInput(string input)
        {
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
