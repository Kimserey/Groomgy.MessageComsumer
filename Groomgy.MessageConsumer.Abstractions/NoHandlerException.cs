using System;

namespace Groomgy.MessageConsumer.Abstractions
{
    public class NoHandlerException : Exception
    {
        public NoHandlerException(string message) : base(message) { }
    }

    public class DecoderException : Exception
    {
        public DecoderException(string message, Type decoderType) : base(message)
        {
            DecoderType = decoderType;
        }
        public Type DecoderType { get; }
    }

    public class HandlerException : Exception
    {
        public HandlerException(string message, Type handlerType) : base(message)
        {
            HandlerType = handlerType;
        }
        public Type HandlerType { get; }
    }
}