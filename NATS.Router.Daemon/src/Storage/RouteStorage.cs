using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NATS.Router.Daemon.Storage
{
    public class RouteStorage
    {
        private Dictionary<string, string> routes;
        private ILogger<RouteStorage> logger;
        private object locker;
        private RouteStorage(ILogger<RouteStorage> logger)
        {
            this.logger = logger;
            locker = new object();
        }
        private void init()
        {
            string routesJson = File.ReadAllText("routes.json");
            routes = JsonSerializer.Deserialize<Dictionary<string, string>>(routesJson);

        }
        public void Add(string upStream, string downStream)
        {
            lock (locker)
            {
                if (routes.TryAdd(upStream, downStream))
                {
                    File.WriteAllText("routes.json", JsonSerializer.Serialize(routes));
                }
                else
                {
                    logger.LogError("Duplicate key", upStream, downStream);
                }
            }
        }
        public void Update(string upStream, string downStream)
        {
            lock (locker)
            {
                if (routes.TryGetValue(upStream, out string value))
                {
                    routes[upStream] = downStream;
                    File.WriteAllText("routes.json", JsonSerializer.Serialize(routes));
                }
                else
                {
                    logger.LogError("Key not found", upStream);
                }
            }
        }
        public void Delete(string upStream)
        {
            lock (locker)
            {
                if (routes.TryGetValue(upStream, out string value))
                {
                    routes.Remove(upStream);
                    File.WriteAllText("routes.json", JsonSerializer.Serialize(routes));
                }
                else
                {
                    logger.LogError("Key not found", upStream);
                }
            }
        }
    }
}