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

            var argsList = HandleQoutes(args);

            return (command, argsList);
        }

        private static List<string> HandleQoutes(string args)
        {
            var inSingleQuotes = false;
            var inDoubleQuotes = false;
            var currentArg = string.Empty;
            var argList = new List<string>();

            foreach (var ch in args)
            {
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

                if (ch == '\"')
                {
                    inDoubleQuotes = !inDoubleQuotes;
                    continue;
                }

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
