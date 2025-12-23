namespace MyShell.Core
{
    public static class Parser
    {
        public static (string command, List<string> args) ParseInput(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts.Length > 0 ? parts[0] : string.Empty;
            var args = parts.Skip(1).ToList();
            return (command, args);
        }
    }
}
