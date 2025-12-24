namespace MyShell.Core
{
    public static class Parser
    {
        public static (string command, List<string> args) ParseInput(string input)
        {
            var spaceIndex = input.IndexOf(' ');
            if (spaceIndex == -1)
            {
                return (input, new List<string>());
            }

            var command = input[..spaceIndex];

            var args = input.Substring(spaceIndex + 1);

            var argsList = HandleQuotes(args);

            return (command, argsList);
        }

        private static List<string> HandleQuotes(string args)
        {
            var inSingleQuotes = false;
            var inDoubleQuotes = false;
            var escaped = false;
            var currentArg = string.Empty;
            var argList = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                var ch = args[i];

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
                        if (i + 1 < args.Length && (args[i + 1] == '\"' || args[i + 1] == '\\'))
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
