using System;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{

    class Program
    {
        static void Main(string[] args)
        {
            using var consumer = new Consumer();

            var host = new Host<string>(consumer)
                .ConfigureLogger(builder => builder.AddConsole())
                .ConfigureServices((config, services) => { services.AddScoped<INameService, NameService>(); })
                .Map<PathFilter>(pathBuilder =>
                {
                    pathBuilder
                        .AddDecoder<Message, MessageDecoder>()
                        .AddHandler<Message, MessageHandler>();
                })
                .Map<HelloFilter>(pathBuilder =>
                {
                    pathBuilder
                        .AddDecoder<Hello, HelloDecoder>()
                        .AddHandler<Message, MessageHandler>()
                        .AddHandler<Hello, HelloHandler>();
                });

            host.Start();

            while (true)
            {
                Console.WriteLine(">> Type the message to send 1) Path1 2) Path2");
                var line = Console.ReadLine();

                switch (line)
                {
                    case "1":
                        consumer.Send(JsonConvert.SerializeObject(new Message { Content = "hehe" }));
                        break;
                    case "2":
                        consumer.Send(JsonConvert.SerializeObject(new Hello { Text = "Hello!"}));
                        break;
                }
            }
        }
    }
}