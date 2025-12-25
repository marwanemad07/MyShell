namespace MyShell.Core.Commands
{
    public class EchoCommand : ICommand
    {
        public string Name => "echo";

        public int Execute(List<string> args)
        {
            if (Helper.IsOutputRedirection(args))
            {
                return Helper.HandleOutputRedirection(args[0], args[2]);
            }

            Console.Write(string.Join(' ', args));

            Console.WriteLine();
            return 0;
        }
    }
}
