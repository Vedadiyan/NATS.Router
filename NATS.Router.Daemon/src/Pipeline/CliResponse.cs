using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace NATS.Router.Daemon.Pipeline
{
    public class CliResponse : IDisposable
    {
        private readonly NamedPipeClientStream namedPipeClientStream;
        public CliResponse(string pipeName)
        {
            namedPipeClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        }
        public async Task WriteAsync(string str)
        {
            try
            {
                await namedPipeClientStream.ConnectAsync(2000);
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Timeout");
                return;
            }
            using (StreamWriter streamWriter = new StreamWriter(namedPipeClientStream))
            {
                await streamWriter.WriteLineAsync("NATS.Router.Daemon");
                await streamWriter.WriteAsync(str);
            }
        }
        public void Dispose()
        {
            namedPipeClientStream.Close();
            namedPipeClientStream.Dispose();
        }
    }
}