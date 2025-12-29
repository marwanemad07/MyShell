namespace MyShell.Core.Commands
{
    public class HistoryCommand : ICommand
    {
        public string Name => "history";

        public readonly List<string> History = new List<string>();

        public int Execute(List<string> args)
        {
            int n =
                args.Count > 0 && int.TryParse(args[0], out var parsedN) ? parsedN : History.Count;
            n = Math.Min(n, History.Count);

            for (int i = History.Count - n; i < History.Count; i++)
            {
                Console.WriteLine($"{i + 1, 4}  {History[i]}");
            }

            return 0;
        }
    }
}
