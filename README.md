## A framework to consume messages.

Provide a scoped consumption of messages. 

Quick sample of a host definition:

```c#
var host = new Host<string>(consumer)
    .ConfigureLogger(builder => builder.AddConsole())
    .ConfigureServices((config, services) => { services.AddScoped<INameService, NameService>(); })
    .Map<PathFilter>(pathBuilder =>
    {
        // Filtering path will determine which path of decoder/handler to look for.
        pathBuilder
            .AddDecoder<Message, MessageDecoder>()
            .AddHandler<Message, MessageHandler>();
    })
    .Map<HelloFilter>(pathBuilder =>
    {
        // For example here HelloDecoder will dictate that HelloHandler will be used.
        // As MessageHandler doesn't handle Hello type.
        pathBuilder
            .AddDecoder<Hello, HelloDecoder>()
            .AddHandler<Message, MessageHandler>()
            .AddHandler<Hello, HelloHandler>();
    });
```

- Path Filter
- Decoder
- Handler

### `Path Filter`

A `Path Filter` determines which group of decoders/handlers to use. It resembles to a `Route` setup in a web framework.
When the filter returns true, only the decoders/handlers configured are considered.

```c#
    public class HelloFilter : PathFilterBase<string>
    {
        private readonly ILogger<HelloFilter> _logger;

        public HelloFilter(ILogger<HelloFilter> logger)
        {
            _logger = logger;
        }

        public override Task<bool> Filter(string message)
        {
            _logger.LogInformation("Hello!");
            return Task.FromResult(message.Contains("Hello"));
        }
    }
```

The filter supports dependency injection. Inherit from `PathFilterBase` as the `Context` property will be available.

### `Decoder`

```c#
public class HelloDecoder : DecoderBase<string, Hello>
{
    private readonly ILogger<HelloDecoder> _logger;

    public HelloDecoder(ILogger<HelloDecoder> logger)
    {
        _logger = logger;
    }

    public override Task<bool> CanDecode(string raw)
    {
        return Task.FromResult(true);
    }

    public override Task<bool> Decode(string raw, out Hello mapped)
    {
        mapped = JsonConvert.DeserializeObject<Hello>(raw);
        _logger.LogInformation("Successfully mapped Hello world");
        return Task.FromResult(true);
    }
}
```

### `Handler`

```c#
public class HelloHandler : HandlerBase<Hello>
{
    private readonly ILogger<HelloHandler> _logger;

    public HelloHandler(ILogger<HelloHandler> logger)
    {
        _logger = logger;
    }

    public override Task<bool> CanHandle(Hello message)
    {
        return Task.FromResult(true);
    }

    public override Task<bool> Handle(Hello mapped)
    {
        _logger.LogInformation("Handled {mapped}", mapped);
        return Task.FromResult(true);
    }
}
```