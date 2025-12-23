namespace MyShell.Core.Commands
{
    public class PwdCommand : ICommand
    {
        public string Name => "pwd";

        public int Execute(List<string> args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            return 0;
        }
    }
}
