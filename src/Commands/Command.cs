namespace MyShell.Core.Commands
{
    public interface ICommand
    {
        string Name { get; }
        int Execute(List<string> args);
    }
}
