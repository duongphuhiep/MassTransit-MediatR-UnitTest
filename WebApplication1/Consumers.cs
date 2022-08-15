using MassTransit;

namespace WebApplication1;

public class Consumer13 : IConsumer<Input1>
{
    private readonly Context _dbContext;
    public Consumer13(Context dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await Task.Delay(10);
        var responseMsg = $"From {context.Message.Info}, Consumer13 is called";
        await _dbContext.Input1s!.AddAsync(new Input1History { Input = context.Message.Info, Output = responseMsg });
        await context.RespondAsync(new Output1 { Info = responseMsg });

        _dbContext.SaveChanges();
    }
}

public class Consumer12 : IConsumer<Input1>
{
    private readonly Context _dbContext;
    public Consumer12(Context dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<Input1> context)
    {
        var responseMsg = $"From {context.Message.Info}, Consumer12 is called";
        await _dbContext.Input1s!.AddAsync(new Input1History { Input = context.Message.Info, Output = responseMsg });
        await context.RespondAsync(new Output1 { Info = responseMsg });
        _dbContext.SaveChanges();
    }
}

public class Consumer21 : IConsumer<Input2>
{
    private readonly Context _dbContext;
    private readonly IRequestClient<Input1> _requestClient1;

    public Consumer21(IRequestClient<Input1> requestClient12, Context dbContext)
    {
        _requestClient1 = requestClient12;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<Input2> context)
    {
        //get the response from Consumer12 or Consumer13 depend on who will responds first
        var responseConsumer1 = await _requestClient1.GetResponse<Output1>(new Input1 { Info = "Input1" });

        var responseMsg = $"From {context.Message.Info}, Consumer21 is called. {responseConsumer1.Message.Info}";
        await _dbContext.Input2s!.AddAsync(new Input2History { Input = context.Message.Info, Output = responseMsg });
        await context.RespondAsync(new Output2 { Info = responseMsg });
        _dbContext.SaveChanges();
    }
}