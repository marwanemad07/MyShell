using System.Diagnostics.Contracts;
using System.Threading.Tasks.Dataflow;

namespace MyShell.Core.Commands
{
    public class EchoCommand : ICommand
    {
        public string Name => "echo";

        public int Execute(List<string> args)
        {
            var redirectionOptions = RedirectionOptions.Parse(args);

            if (redirectionOptions.HasOutputRedirection)
            {
                FileWriter.Write(args[0], redirectionOptions.TargetFile!);
                return 0;
            }
            else if (redirectionOptions.AppendOutput)
            {
                FileWriter.Write(args[0], redirectionOptions.TargetFile!, append: true);
                return 0;
            }

            if (redirectionOptions.HasErrorRedirection)
            {
                FileWriter.Write(null, args[^1]);
                args = args.Take(args.Count - 2).ToList();
            }
            else if (redirectionOptions.AppendError)
            {
                FileWriter.Write(null, args[^1], append: true);
                args = args.Take(args.Count - 2).ToList();
            }

            Console.Write(string.Join(' ', args));

            Console.WriteLine();
            return 0;
        }
    }
}
