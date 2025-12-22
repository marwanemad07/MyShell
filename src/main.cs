class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var command = Console.ReadLine()?.Trim();
            Console.WriteLine($"{command}: command not found");
        }
    }
}
