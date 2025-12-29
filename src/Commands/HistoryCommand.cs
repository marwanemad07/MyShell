namespace MyShell.Core.Commands
{
    public class HistoryCommand : ICommand
    {
        public string Name => "history";

        public readonly List<string> History = new List<string>();

        public int Execute(List<string> args)
        {
            for (int i = 0; i < History.Count; i++)
            {
                Console.WriteLine($"{i + 1, 4}  {History[i]}");
            }

            return 0;
        }
    }
}
