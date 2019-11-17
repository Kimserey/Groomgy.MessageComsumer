using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{
    public class Consumer: IConsumer<string>, IDisposable
    {
        private readonly  Subject<string> _subject;

        public Consumer()
        {
            _subject = new Subject<string>();
        }

        public void Send(string message)
        {
            _subject.OnNext(JsonConvert.SerializeObject(new Message { Content = message }));
        }

        public Task Consume(Action<string> consume)
        {
            _subject.Subscribe(consume);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _subject?.Dispose();
        }
    }
}