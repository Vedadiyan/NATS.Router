using System;
using System.Threading.Tasks;

namespace NATS.Router.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(await CliRequest.SendCommandAsync(string.Join(' ', args)));
        }
    }
}
