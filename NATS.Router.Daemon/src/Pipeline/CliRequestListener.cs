using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace NATS.Router.Daemon.Pipeline
{
    public class CliRequestListener : IDisposable
    {
        private readonly NamedPipeServerStream namedPipeServerStream;
        private readonly StreamReader streamReader;
        private CliRequestListener()
        {
            namedPipeServerStream = new NamedPipeServerStream("NATS.Router.Daemon", PipeDirection.In, 1);
            streamReader = new StreamReader(namedPipeServerStream);
        }
        public static Task ListenAsync(Func<string, string> handleRequest, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                CliRequest request = null;
                while ((request = await next(cancellationToken)) != null)
                {
                    using (CliResponse response = new CliResponse(request.ClientId))
                    {
                        await response.WriteAsync(handleRequest(request.Message) ?? "Invalid command.\r\nPlease use -h to view a list of available commands");
                    }
                }
            });
        }
        public void Dispose()
        {
            namedPipeServerStream.Close();
            namedPipeServerStream.Dispose();
        }
        private static async Task<CliRequest> next(CancellationToken cancellationToken)
        {
            using (CliRequestListener cliRequestListener = new CliRequestListener())
            {
                await cliRequestListener.namedPipeServerStream.WaitForConnectionAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return default;
                }
                return new CliRequest(await cliRequestListener.streamReader.ReadLineAsync(), await cliRequestListener.streamReader.ReadLineAsync());
            }

        }
    }
}