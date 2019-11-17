using System;
using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            using var consumer = new Consumer();

            var host = new Host<string>(consumer)
                .ConfigureServices((config, services) => { })
                .ConfigureLogger(builder => builder.AddConsole())
                .Map<PathFilter>(pathBuilder =>
                {
                    pathBuilder
                        .AddDecoder<Message, MessageDecoder>()
                        .AddHandler<Message, MessageHandler>();
                });

            host.Start();

            while (true)
            {
                var line = Console.ReadLine();
                consumer.Send(line);
            }
        }
    }

    public class PathFilter: IPathFilter<string>
    {
        public Task<bool> Filter(string message)
        {
            return Task.FromResult(true);
        }
    }
}