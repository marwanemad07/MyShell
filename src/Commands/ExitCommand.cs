namespace MyShell.Core.Commands
{
    public class ExitCommand : ICommand
    {
        public string Name => "exit";

        public int Execute(List<string> args)
        {
            Environment.Exit(0);
            return 0;
        }
    }
}
