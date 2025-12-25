namespace MyShell.Core.Commands
{
    public class EchoCommand : ICommand
    {
        public string Name => "echo";

        public int Execute(List<string> args)
        {
            if (Helper.IsOutputRedirection(args))
            {
                Helper.WriteToFile(args[0], args[2]);
                return 0;
            }

            if (Helper.IsErrorRedirection(args))
            {
                Helper.WriteToFile(null, args[^1]);
                args = args.Take(args.Count - 2).ToList();
            }

            Console.Write(string.Join(' ', args));

            Console.WriteLine();
            return 0;
        }
    }
}
