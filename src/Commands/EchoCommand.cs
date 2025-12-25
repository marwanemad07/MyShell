namespace MyShell.Core.Commands
{
    public class EchoCommand : ICommand
    {
        public string Name => "echo";

        public int Execute(List<string> args)
        {
            bool outputRedirection = Helper.IsOutputRedirection(args);
            bool errorRedirection = Helper.IsErrorRedirection(args);
            bool appendOutputRedirection = Helper.IsAppednOutputRedirection(args);

            if (outputRedirection)
            {
                Helper.WriteToFile(args[0], args[2]);
                return 0;
            }
            else if (appendOutputRedirection)
            {
                Helper.WriteToFile(args[0], args[2], append: true);
                return 0;
            }

            if (errorRedirection)
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
