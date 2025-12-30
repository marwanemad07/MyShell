namespace MyShell.Core.Commands
{
    public class ExitCommand : ICommand
    {
        private readonly HistoryCommand? _historyCommand;

        public string Name => "exit";

        public ExitCommand(HistoryCommand? historyCommand = null)
        {
            _historyCommand = historyCommand;
        }

        public int Execute(List<string> args)
        {
            WriteHistoryToFile();
            Environment.Exit(0);
            return 0;
        }

        private void WriteHistoryToFile()
        {
            var histFile = Environment.GetEnvironmentVariable("HISTFILE");
            if (string.IsNullOrEmpty(histFile) || _historyCommand == null)
            {
                return;
            }

            var content =
                string.Join(Environment.NewLine, _historyCommand.History) + Environment.NewLine;
            File.WriteAllText(histFile, content);
        }
    }
}
