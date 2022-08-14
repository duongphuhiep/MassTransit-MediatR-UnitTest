using App.Models;
using MassTransit;

namespace App.Demo2;

public class Consumer11 : IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await context.RespondAsync(new Output1("1A"));
        await context.RespondAsync(new Output1("1B"));
        await context.RespondAsync(new Output1("1C"));
        await context.RespondAsync(new Output2("2A"));
        await context.RespondAsync(new Output2("2B"));
        await context.RespondAsync(new Output2("2C"));
    }
}

public class Consumer12 : IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await context.RespondAsync(new Output1(context.Message.Info + " World"));
    }
}

public class Consumer21 : IConsumer<Input2>
{
    private readonly IRequestClient<Input1> _requestClient12;

    public Consumer21(IRequestClient<Input1> requestClient12)
    {
        _requestClient12 = requestClient12;
    }

    public async Task Consume(ConsumeContext<Input2> context)
    {
        var response = await _requestClient12.GetResponse<Output1>(new Input1(1, "Hello"));
        await context.RespondAsync(new Output2("Greetings " + response.Message.Info));
    }
}