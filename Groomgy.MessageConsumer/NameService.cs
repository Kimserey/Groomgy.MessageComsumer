using System;
using System.Collections.Generic;

namespace Groomgy.MessageConsumer
{
    public interface INameService: IDisposable
    {
        string GetName();
    }

    public class NameService : INameService
    {
        private readonly List<string> _data;

        public NameService()
        {
            _data = new List<string>();
        }

        public string GetName()
        {
            _data.Add("Hehe");
            return string.Join(" ", _data);
        }

        public void Dispose()
        {
            Console.WriteLine($"{this.GetType().Name} disposed");
        }
    }
}