namespace MyShell.Core.Commands
{
    public class HistoryCommand : ICommand
    {
        public string Name => "history";

        public readonly List<string> History = new List<string>();

        public int Execute(List<string> args)
        {
            if (args.Count > 0 && args[0] == "-r")
            {
                if (args.Count < 2)
                {
                    Console.WriteLine("history: option requires an argument");
                    return 1;
                }

                var filePath = args[1];
                if (File.Exists(filePath))
                {
                    try
                    {
                        var lines = File.ReadAllLines(filePath);
                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                History.Add(line);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"history: {ex.Message}");
                        return 1;
                    }
                }
                else
                {
                    Console.WriteLine($"history: {filePath}: No such file or directory");
                    return 1;
                }
                return 0;
            }

            int n =
                args.Count > 0 && int.TryParse(args[0], out var parsedN) ? parsedN : History.Count;
            n = Math.Min(n, History.Count);

            for (int i = History.Count - n; i < History.Count; i++)
            {
                Console.WriteLine($"{i + 1, 4}  {History[i]}");
            }

            return 0;
        }
    }
}
