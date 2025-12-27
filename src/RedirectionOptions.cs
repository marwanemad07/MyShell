namespace MyShell.Core
{
    public class RedirectionOptions
    {
        public bool HasOutputRedirection { get; set; }
        public bool HasErrorRedirection { get; set; }
        public bool AppendOutput { get; set; }
        public bool AppendError { get; set; }
        public string? TargetFile { get; set; }

        public bool HasAnyRedirection =>
            HasOutputRedirection || HasErrorRedirection || AppendOutput || AppendError;

        public static RedirectionOptions Parse(List<string> args)
        {
            return new RedirectionOptions
            {
                HasOutputRedirection = args.Contains("1>") || args.Contains(">"),
                HasErrorRedirection = args.Contains("2>"),
                AppendOutput = args.Contains("1>>") || args.Contains(">>"),
                AppendError = args.Contains("2>>"),
                TargetFile = GetTargetFile(args),
            };
        }

        private static string? GetTargetFile(List<string> args)
        {
            if (args.Count < 2)
                return null;

            // check if last argument is a redirection operator
            var lastArg = args[^2];
            if (IsRedirectionOperator(lastArg))
                return args[^1];

            return null;
        }

        private static bool IsRedirectionOperator(string arg)
        {
            return arg == ">"
                || arg == ">>"
                || arg == "1>"
                || arg == "1>>"
                || arg == "2>"
                || arg == "2>>";
        }

        public List<string> GetFilteredArgs(List<string> args)
        {
            if (!HasAnyRedirection)
                return args;

            // remove redirection operator and target file (last 2 arguments)
            return args.Take(args.Count - 2).ToList();
        }
    }
}
