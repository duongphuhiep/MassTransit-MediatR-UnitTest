using MassTransit;

namespace WebApplication1;

public class Consumer13 : IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await Task.Delay(10);
        await context.RespondAsync(new Output1 { Info = $"From {context.Message.Info}, Consumer13 is called" });
    }
}

public class Consumer12 : IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await context.RespondAsync(new Output1 { Info = $"From {context.Message.Info}, Consumer12 is called" });
    }
}

public class Consumer21 : IConsumer<Input2>
{
    private readonly IRequestClient<Input1> _requestClient1;

    public Consumer21(IRequestClient<Input1> requestClient12)
    {
        _requestClient1 = requestClient12;
    }

    public async Task Consume(ConsumeContext<Input2> context)
    {
        //get the response from Consumer12 or Consumer13 depend on who will responds first
        var response = await _requestClient1.GetResponse<Output1>(new Input1 { Info = "Input1" });
        await context.RespondAsync(new Output2 { Info = $"From {context.Message.Info}, Consumer21 is called. {response.Message.Info}" });
    }
}