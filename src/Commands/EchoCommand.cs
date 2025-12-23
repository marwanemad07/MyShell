namespace MyShell.Core.Commands
{
    public class EchoCommand : ICommand
    {
        public string Name => "echo";

        public int Execute(List<string> args)
        {
            Console.Write(string.Join(' ', args));

            Console.WriteLine();
            return 0;
        }
    }
}
