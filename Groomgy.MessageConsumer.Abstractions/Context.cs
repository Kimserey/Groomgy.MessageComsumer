using System.Collections.Generic;

namespace Groomgy.MessageConsumer.Abstractions
{
    public class Context: Dictionary<string ,string>
    {
        public string CorrelationId { get; set; }
    }
}