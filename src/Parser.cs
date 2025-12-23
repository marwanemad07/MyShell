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

            var argsList = HandleSingleQoutes(args);

            return (command, argsList);
        }

        private static List<string> HandleSingleQoutes(string args)
        {
            var inSingleQuotes = false;
            var currentArg = string.Empty;
            var argList = new List<string>();

            foreach (var ch in args)
            {
                if (ch == '\'')
                {
                    inSingleQuotes = !inSingleQuotes;
                    continue;
                }

                if (ch == ' ' && !inSingleQuotes)
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

            if (!inSingleQuotes && !string.IsNullOrEmpty(currentArg))
            {
                argList.Add(currentArg);
            }

            // TODO: Handle unclosed quotes if necessary

            return argList;
        }
    }
}
