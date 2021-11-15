using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace NATS.Router.CLI
{
    public class CliRequest : IDisposable
    {
        private readonly string clientId;
        private readonly NamedPipeServerStream namedPipeServerStream;
        private readonly NamedPipeClientStream namedPipeClientStream;
        private CliRequest()
        {
            clientId = Guid.NewGuid().ToString();
            namedPipeServerStream = new NamedPipeServerStream(clientId, PipeDirection.In, 1);
            namedPipeClientStream = new NamedPipeClientStream(".", "NATS.Router.Daemon", PipeDirection.Out);
        }
        public static async Task<string> SendCommandAsync(string command)
        {
            using (CliRequest cliRequest = new CliRequest())
            {
                Task server = cliRequest.namedPipeServerStream.WaitForConnectionAsync();
                try
                {
                    await cliRequest.namedPipeClientStream.ConnectAsync(2000);
                    using (StreamWriter streamWriter = new StreamWriter(cliRequest.namedPipeClientStream))
                    {
                        await streamWriter.WriteLineAsync(cliRequest.clientId);
                        await streamWriter.WriteLineAsync(command);
                    }
                    await server;
                    using (StreamReader streamReader = new StreamReader(cliRequest.namedPipeServerStream))
                    {
                        await streamReader.ReadLineAsync();
                        return await streamReader.ReadToEndAsync();
                    }
                }
                catch (TimeoutException)
                {
                    return "NATS Router Daemon is not running.";
                }
            }
        }

        public void Dispose()
        {
            namedPipeServerStream.Close();
            namedPipeServerStream.Dispose();
            namedPipeClientStream.Close();
            namedPipeClientStream.Dispose();
        }
    }
}