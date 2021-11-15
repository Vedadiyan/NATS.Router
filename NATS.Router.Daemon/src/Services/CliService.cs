using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NATS.Router.Daemon.Annotations;
using NATS.Router.Daemon.Pipeline;

namespace NATS.Router.Daemon.Services
{
    public class CliService : BackgroundService
    {
        private static IReadOnlyDictionary<string, Func<string[], string>> methods;
        static CliService()
        {
            Dictionary<string, Func<string[], string>> _methods = new Dictionary<string, Func<string[], string>>();
            MethodInfo[] methodInfos = typeof(CliService).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var i in methodInfos)
            {
                CommandHandlerAttribute commandHandlerAttribute = i.GetCustomAttribute<CommandHandlerAttribute>();
                if (i.ReturnType == typeof(string) && commandHandlerAttribute != null)
                {
                    _methods.Add(commandHandlerAttribute.CommandName, (x) => (string)i.Invoke(null, new object[] { x }));
                }
            }
            methods = _methods;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Regex pattern = new Regex("\\s|(\"[^\"]*\")");
            await CliRequestListener.ListenAsync((x) =>
            {
                string[] command = pattern.Split(x).Where(y => y.Length > 1).ToArray();
                if (methods.TryGetValue(command[0], out Func<string[], string> func))
                {
                    return func(command);
                }
                return null;
            }, stoppingToken);
        }
        [CommandHandler("help")]
        public static string Help(string[] args) {
            return "Help is in progress";
        }
    }
}