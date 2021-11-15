using System;

namespace NATS.Router.Daemon.Annotations
{
    public class CommandHandlerAttribute : Attribute
    {
        public string CommandName { get; }
        public CommandHandlerAttribute(string commandName) {
            CommandName = commandName;
        }
    }
}