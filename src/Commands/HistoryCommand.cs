using System.Linq;

namespace MyShell.Core.Commands
{
    public class HistoryCommand : ICommand
    {
        public string Name => "history";

        public readonly List<string> History = new List<string>();
        private int _lastPersistedIndex = 0;

        public int Execute(List<string> args)
        {
            if (args.Count == 0)
            {
                return PrintHistory(History.Count);
            }

            return args[0] switch
            {
                "-r" => HandleReadOption(args),
                "-w" => HandleWriteOption(args),
                "-a" => HandleAppendOption(args),
                _ => HandlePrintWithLimit(args[0]),
            };
        }

        public void LoadHistoryFromFile()
        {
            var histFile = Environment.GetEnvironmentVariable("HISTFILE");
            if (string.IsNullOrEmpty(histFile) || !File.Exists(histFile))
            {
                return;
            }

            var lines = File.ReadAllLines(histFile);
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    History.Add(line);
                }
            }
        }

        private int HandleReadOption(List<string> args)
        {
            if (args.Count < 2)
            {
                Console.WriteLine("history: option requires an argument");
                return 1;
            }

            var filePath = args[1];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"history: {filePath}: No such file or directory");
                return 1;
            }

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

            _lastPersistedIndex = History.Count;
            return 0;
        }

        private int HandleWriteOption(List<string> args)
        {
            if (args.Count < 2)
            {
                Console.WriteLine("history: option requires an argument");
                return 1;
            }

            var filePath = args[1];
            try
            {
                var content = string.Join(Environment.NewLine, History) + Environment.NewLine;
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"history: {ex.Message}");
                return 1;
            }

            _lastPersistedIndex = History.Count;
            return 0;
        }

        private int HandleAppendOption(List<string> args)
        {
            if (args.Count < 2)
            {
                Console.WriteLine("history: option requires an argument");
                return 1;
            }

            var entriesToAppend = History.Count - _lastPersistedIndex;
            if (entriesToAppend <= 0)
                return 0;

            var filePath = args[1];
            try
            {
                var slice = History.Skip(_lastPersistedIndex).Take(entriesToAppend).ToList();
                var content = string.Join(Environment.NewLine, slice) + Environment.NewLine;
                File.AppendAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"history: {ex.Message}");
                return 1;
            }

            _lastPersistedIndex = History.Count;
            return 0;
        }

        private int HandlePrintWithLimit(string limitArg)
        {
            if (int.TryParse(limitArg, out var limit))
            {
                limit = Math.Max(limit, 0);
                return PrintHistory(limit);
            }

            // fall back to printing all history if parsing fails
            return PrintHistory(History.Count);
        }

        private int PrintHistory(int entriesToShow)
        {
            var count = Math.Min(entriesToShow, History.Count);
            for (int i = History.Count - count; i < History.Count; i++)
            {
                Console.WriteLine($"{i + 1, 4}  {History[i]}");
            }

            return 0;
        }
    }
}
