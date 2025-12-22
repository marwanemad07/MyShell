class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var command = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(command) || command.ToLower() == "exit")
                break;

            Console.WriteLine($"{command}: command not found");
        }
    }
}
