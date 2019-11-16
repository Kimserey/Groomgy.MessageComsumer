using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{
    public interface IHandler<in TMessage>
    {
        Task<bool> CanHandle(TMessage raw);

        Task Handle(TMessage message);
    }

    public interface IMapper<TMessage>
    {
        Task<bool> Map(string raw, out TMessage mapped);
    }

    public interface IHost
    {
        IHost ConfigureServices(Action<IServiceCollection> configureServices);

        IHost AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>;

        IHost AddMapper<TMessage, TMapper>()
            where TMapper: IMapper<TMessage>;

        void Start();
    }

    public interface IFakeConsumer
    {
        Task Consume(Action<string> consume);
    }

    public class FakeConsumer: IFakeConsumer
    {
        public Task Consume(Action<string> consume)
        {
            throw new NotImplementedException();
        }
    }

    public class Host: IHost
    {
        private readonly IFakeConsumer _consumer;
        private readonly IServiceCollection _services;
        private readonly List<(Type, MethodInfo, MethodInfo)> _handlerMethods = new List<(Type, MethodInfo, MethodInfo)>();
        private readonly List<(Type, MethodInfo)> _mapperMethods = new List<(Type, MethodInfo)>();

        public Host(IFakeConsumer consumer)
        {
            _services = new ServiceCollection();
            _consumer = consumer;
        }

        public IHost ConfigureServices(Action<IServiceCollection> configureServices)
        {
            configureServices(_services);
            return this;
        }

        public IHost AddHandler<TMessage, THandler>() where THandler : IHandler<TMessage>
        {
            _handlerMethods.Add((
                typeof(THandler),
                typeof(THandler).GetMethod("CanHandle"), 
                typeof(THandler).GetMethod("Handle")
            ));
            return this;
        }

        public IHost AddMapper<TMessage, TMapper>() where TMapper : IMapper<TMessage>
        {
            _mapperMethods.Add((
                typeof(TMapper),
                typeof(TMapper).GetMethod("Map")
            ));
            return this;
        }

        public void Start()
        {
            var provider = _services.BuildServiceProvider();

            _consumer.Consume(async obj =>
            {
                using (var scope = provider.CreateScope())
                {
                    foreach (var (mapperType, mapFunc) in _mapperMethods)
                    {
                        var mapper =
                            ActivatorUtilities.CreateInstance(scope.ServiceProvider, mapperType);

                        var args = new object[2];
                        args[0] = obj;
                        var isMapped =
                            await (Task<bool>) mapFunc.Invoke(mapper, args);

                        if (!isMapped)
                            continue;

                        var mapped = args[1];

                        foreach (var (handlerType, canHandleFunc, handleFunc) in _handlerMethods)
                        {
                            var handler =
                                ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType);

                            var canHandle =
                                await (Task<bool>) canHandleFunc.Invoke(handler, new[] {mapped});

                            if (canHandle)
                                await (Task) handleFunc.Invoke(handler, new[] {mapped});
                        }
                    }
                }
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var fake = new FakeConsumer();

            var host = new Host(fake)
                .ConfigureServices(services => { })
                .AddHandler<Message, MessageHandler>();

            host.Start();
        }
    }
}