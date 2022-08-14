using MassTransit;

namespace WebApplication1;

public class Consumer13 : IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await Task.Delay(10);
        await context.RespondAsync(new Output1 {Info = "Keke"});
    }
}

public class Consumer12 : IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await context.RespondAsync(new Output1 {Info = context.Message.Info + " World"});
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
        var response = await _requestClient12.GetResponse<Output1>(new Input1 {Info = "Hello"});
        await context.RespondAsync(new Output2 {Info = "Greetings " + response.Message.Info});
    }
}