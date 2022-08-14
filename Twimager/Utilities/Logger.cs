using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Twimager.Utilities
{
    public class Logger
    {
        private readonly StreamWriter _writer;

        public Logger(string path = null)
        {
            if (path == null) return;

            _writer = File.AppendText(path);
            _writer.AutoFlush = true;
        }

        ~Logger()
        {
            _writer.Close();
        }

        public async Task LogAsync(string message)
        {
            message = AppendDateTime(message);
            Console.WriteLine(message);

            if (_writer == null) return;
            await _writer.WriteLineAsync(message);
        }

        public async Task LogExceptionAsync(Exception e)
        {
            var inner = e;
            while (inner != null)
            {
                await LogAsync($"{(inner == e ? "" : "Caused by: ")}{e.GetType().FullName}: {e.Message}");
                await LogAsync(e.StackTrace);

                inner = e.InnerException;
            }
        }

        private string AppendDateTime(string message)
        {
            return $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: {message}";
        }
    }
}
