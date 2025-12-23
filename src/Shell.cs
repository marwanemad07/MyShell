using myshell.src.Commands;

namespace myshell.src
{
    public class Shell
    {
        public Shell() { }

        public void Run()
        {
            while (true)
            {
                Console.Write("$ ");

                var input = Console.ReadLine()?.Trim();

                var (command, args) = Parser.ParseInput(input ?? string.Empty);

                if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                    break;

                if (command == "echo")
                {
                    var echoCommand = new EchoCommand();

                    echoCommand.Execute(args);
                    continue;
                }

                Console.WriteLine($"{command}: command not found");
            }
        }
    }
}
