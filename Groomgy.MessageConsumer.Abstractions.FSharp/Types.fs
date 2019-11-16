namespace Groomgy.MessageConsumer.Abstractions.FSharp

type IConsumer =
    abstract Consume: string -> unit -> Async<unit>