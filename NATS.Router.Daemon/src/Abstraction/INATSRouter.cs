namespace NATS.Router.Daemon.Abstraction {
    public interface INATSRouter {
        string GetDownStream(string upStream);
    }
}