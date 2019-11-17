using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer.Abstractions
{
    public class Host<TRaw>: IHost<TRaw>
    {
        private readonly IConsumer<TRaw> _consumer;
        
        private readonly IServiceCollection _services;
        private readonly IConfiguration  _configuration;

        private readonly List<PathMeta> _paths =
            new List<PathMeta>();

        public Host(IConsumer<TRaw> consumer)
        {
            _consumer = consumer;

            _services = new ServiceCollection();

            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
        }

        public IHost<TRaw> ConfigureLogger(Action<ILoggingBuilder> configureLogger)
        {
            _services.AddLogging(configureLogger);
            return this;
        }

        public IHost<TRaw> ConfigureServices(Action<IConfiguration, IServiceCollection> configureServices)
        {
            _services.AddScoped<Context>();
            configureServices(_configuration, _services);
            return this;
        }

        public IHost<TRaw> Map<TPathFilter>(Action<IPathBuilder<TRaw>> configure) where TPathFilter : IPathFilter<TRaw>
        {
            var builder = new PathBuilder<TRaw>();
            configure(builder);
            _paths.Add(new PathMeta {
                Filter = new PathMeta.FilterMeta {
                    Type = typeof(TPathFilter),
                    Method = typeof(TPathFilter).GetMethod("Filter")
                },
                Handler = builder.Build()
            });
            return this;
        }

        public void Start()
        {
            var provider =
                _services.BuildServiceProvider();

            var sw = new Stopwatch();

            _consumer.Consume(async raw =>
            {
                var filtered = false;
                var handled = false;

                using (var scope = provider.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Host<TRaw>>>();
                    var context = scope.ServiceProvider.GetRequiredService<Context>();
                    sw.Restart();

                    logger.LogInformation(
                        "Starting consuming message. corId={corId} raw={raw}",
                        context.CorrelationId, raw
                    );

                    foreach (var path in _paths)
                    {
                        // Instantiates the filter tied to the mapping.
                        var filter =
                            ActivatorUtilities.CreateInstance(scope.ServiceProvider, path.Filter.Type);

                        path.Filter
                            .Type.GetProperty("Context")
                            ?.SetValue(filter, context);

                        try
                        {
                            // Executes the filtering logic and identify mapped route.
                            filtered =
                                await (Task<bool>) path.Filter.Method.Invoke(filter, new object[] {raw});
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                                "A failure occurred while calling #{filterType}.Filter. corId={corId} " 
                                + "raw={raw} ex={ex}", 
                                path.Filter.Type, context.CorrelationId, raw, ex.Message);
                        }

                        if (!filtered)
                            // Skip current route if not mapped.
                            continue;

                        try
                        {
                            // Handles the message by invoking the path handler.
                            handled =
                                await path.Handler.Handle(scope.ServiceProvider, raw);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                                "A failure occurred while handling message through #{filterType}. corId={corId} " 
                                + "context={context} raw={raw} ex={ex}", 
                                path.Filter.Type, context.CorrelationId, context, raw, ex.Message);
                        }

                        sw.Stop();
                        if (handled)
                        {
                            logger.LogInformation("Successfully consumed message in {elapsedMs} ms. corId={corId} context={context}",
                                sw.ElapsedMilliseconds, context.CorrelationId, context
                            );
                        }

                        // Only one path will be matched.
                        // Therefore any succesful match will complete the loop.
                        break;
                    }
                }
            });
        }

        public class PathMeta
        {
            public FilterMeta Filter { get; set; }

            public IPathHandler<TRaw> Handler { get; set; }

            public class FilterMeta
            {
                public Type Type { get; set; }

                public MethodInfo Method { get; set; }
            }
        }
    }
}