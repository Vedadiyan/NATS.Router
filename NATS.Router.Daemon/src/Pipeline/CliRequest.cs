namespace NATS.Router.Daemon.Pipeline
{
    public class CliRequest
    {
        public string ClientId { get; }
        public string Message { get; }
        public CliRequest(string clientId, string message)
        {
            ClientId = clientId;
            Message = message;
        }
    }
}