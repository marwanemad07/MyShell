namespace MyShell.Core.Commands
{
    public class EchoCommand : ICommand
    {
        public string Name => "echo";

        public int Execute(List<string> args)
        {
            if (args.Count == 3 && (args[1] == "1>" || args[1] == ">"))
            {
                return HandleOutputRedirection(args);
            }

            Console.Write(string.Join(' ', args));

            Console.WriteLine();
            return 0;
        }

        private int HandleOutputRedirection(List<string> args)
        {
            var filePath = args[2];
            try
            {
                using var writer = new StreamWriter(filePath, false);
                writer.WriteLine(args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file '{filePath}': {ex.Message}");
            }
            return 0;
        }
    }
}
