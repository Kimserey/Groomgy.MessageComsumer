namespace Groomgy.MessageConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var fake = new FakeConsumer();

            var host = new Host(fake)
                .ConfigureServices((config, services) => { })
                .AddHandler<Message, MessageHandler>();

            host.Start();
        }
    }
}