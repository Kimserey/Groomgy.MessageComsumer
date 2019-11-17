using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer
{
    public interface INameService: IDisposable
    {
        string GetName();
    }

    public class NameService : INameService
    {
        private readonly List<string> _data;
        private readonly ILogger<NameService> _logger;

        public NameService(ILogger<NameService> logger)
        {
            _logger = logger;
            _data = new List<string>();
        }

        public string GetName()
        {
            _data.Add("Hehe");
            return string.Join(" ", _data);
        }

        public void Dispose()
        {
            _logger.LogInformation("{type} disposed", this.GetType().Name);
        }
    }
}