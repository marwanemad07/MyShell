namespace myshell.src.Commands
{
    public interface Command
    {
        string Name { get; }
        int Execute(List<string> args);
    }
}
