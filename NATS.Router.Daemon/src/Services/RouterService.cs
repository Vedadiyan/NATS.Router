using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;
using NATS.Router.Daemon.Abstraction;
using NATS.Router.Daemon.Configurations;

namespace NATS.Router.Daemon.Services
{
    public class RouterService : BackgroundService
    {
        private IConnection connection;
        private INATSRouter natsRouter;
        private ILogger<RouterService> logger;
        private IAsyncSubscription subscription;
        public RouterService(INATSRouter natsRouter, IOptions<NATSConnectionConfiguration> natsConnectionConfiguration, ILogger<RouterService> logger)
        {
            this.natsRouter = natsRouter;
            connection = new ConnectionFactory().CreateConnection(string.Join(',', natsConnectionConfiguration.Value.URLs));
            this.logger = logger;
            subscription = connection.SubscribeAsync("root.up.*");

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            subscription.Start();
            stoppingToken.Register(() =>
            {
                subscription.Unsubscribe();
            });
            return Task.CompletedTask;
        }
        private async void onMessage(object sender, MsgHandlerEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.Reply))
            {
                connection.Publish(natsRouter.GetDownStream(e.Message.Subject), e.Message.Data);
            }
            else
            {
                e.Message.InProgress();
                Msg response = await connection.RequestAsync(natsRouter.GetDownStream(e.Message.Subject), e.Message.Data);
                e.Message.Respond(response.Data);
            }
        }
    }
}