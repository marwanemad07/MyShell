namespace myshell.src.Commands
{
    public class EchoCommand : Command
    {
        public string Name => "echo";

        public int Execute(List<string> args)
        {
            foreach (var arg in args)
            {
                Console.Write(arg + " ");
            }

            Console.WriteLine();
            return 0;
        }
    }
}
