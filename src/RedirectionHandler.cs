using System.Diagnostics;
using System.Text;

namespace MyShell.Core
{
    public class RedirectionHandler
    {
        private readonly RedirectionOptions _options;
        private readonly StringBuilder _outputBuilder = new();
        private readonly StringBuilder _errorBuilder = new();

        public RedirectionHandler(RedirectionOptions options)
        {
            _options = options;
        }

        public void AttachToProcess(Process process)
        {
            process.OutputDataReceived += OnOutputDataReceived;
            process.ErrorDataReceived += OnErrorDataReceived;
        }

        public void WriteRedirectedOutput()
        {
            if (_options.HasOutputRedirection)
            {
                FileWriter.Write(_outputBuilder.ToString().TrimEnd(), _options.TargetFile!);
            }

            if (_options.AppendOutput)
            {
                FileWriter.Write(
                    _outputBuilder.ToString().TrimEnd(),
                    _options.TargetFile!,
                    append: true
                );
            }

            if (_options.HasErrorRedirection)
            {
                FileWriter.Write(_errorBuilder.ToString().TrimEnd(), _options.TargetFile!);
            }

            if (_options.AppendError)
            {
                FileWriter.Write(_errorBuilder.ToString().TrimEnd(), _options.TargetFile!, append: true);
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (_options.HasOutputRedirection || _options.AppendOutput)
            {
                _outputBuilder.AppendLine(e.Data);
            }
            else
            {
                Console.WriteLine(e.Data);
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (_options.HasErrorRedirection || _options.AppendError)
            {
                _errorBuilder.AppendLine(e.Data);
            }
            else
            {
                Console.Error.WriteLine(e.Data);
            }
        }
    }
}
