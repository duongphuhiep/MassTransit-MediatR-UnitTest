using MassTransit;
using System.Data.Common;

namespace WebApplication1;

public class Consumer13 : IConsumer<Input1>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IScopedSample21 _sampleScoped21;
    private readonly IScopedSample22 _sampleScoped22;

    public Consumer13(IHttpContextAccessor httpContextAccessor, IScopedSample21 sampleScoped21, IScopedSample22 sampleScoped22)
    {
        _httpContextAccessor = httpContextAccessor;
        _sampleScoped21 = sampleScoped21;
        _sampleScoped22 = sampleScoped22;
    }
    public async Task Consume(ConsumeContext<Input1> context)
    {
        var responseMsg = $"{_sampleScoped21.GetId()}/{_sampleScoped22.GetId()} From {context.Message.Info}, Consumer13 is called";

        DbConnection? dbConnection = _httpContextAccessor.HttpContext?.Items["CurrentDbConnection"] as DbConnection;
        if (dbConnection == null) throw new InvalidOperationException("database connection is not setup in the HttpContext");
        using Context dbContext = new Context(dbConnection!);

        await dbContext.Input1s!.AddAsync(new Input1History { Input = context.Message.Info, Output = responseMsg });
        dbContext.SaveChanges();

        await context.RespondAsync(new Output1 { Info = responseMsg });
    }
}


public class Consumer21 : IConsumer<Input2>
{
    private readonly IRequestClient<Input1> _requestClient1;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IScopedSample21 _sampleScoped21;
    private readonly IScopedSample22 _sampleScoped22;

    public Consumer21(IRequestClient<Input1> requestClient1, IHttpContextAccessor httpContextAccessor, IScopedSample21 sampleScoped21, IScopedSample22 sampleScoped22)
    {
        _requestClient1 = requestClient1;
        _httpContextAccessor = httpContextAccessor;
        _sampleScoped21 = sampleScoped21;
        _sampleScoped22 = sampleScoped22;
    }

    public async Task Consume(ConsumeContext<Input2> context)
    {
        throw new InvalidOperationException("Fu fu fu!");
        //get the response from Consumer12 or Consumer13 depend on who will responds first
        var responseConsumer1 = (await _requestClient1.GetResponse<Output1>(new Input1 { Info = "Input1" })).Message;
        //var responseConsumer1 = new Output1 { Info = "Foo" };

        var responseMsg = $"{_sampleScoped21.GetId()}/{_sampleScoped22.GetId()} From {context.Message.Info}, Consumer21 is called. {responseConsumer1.Info}";

        DbConnection? dbConnection = _httpContextAccessor.HttpContext?.Items["CurrentDbConnection"] as DbConnection;
        if (dbConnection == null) throw new InvalidOperationException("database connection is not setup in the HttpContext");
        using Context dbContext = new Context(dbConnection!);

        await dbContext.Input2s!.AddAsync(new Input2History { Input = context.Message.Info, Output = responseMsg });
        dbContext.SaveChanges();

        await context.RespondAsync(new Output2 { Info = responseMsg });
    }
}



public class Consumer31 : IConsumer<Input3>
{
    private readonly ILogger<Consumer31> _logger;

    public Consumer31(ILogger<Consumer31> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Input3> context)
    {
        _logger.LogInformation("31");
        await context.RespondAsync(new Output3 { Info = "Foo" });
    }
}

public class Consumer32 : IConsumer<Input3>
{
    private readonly ILogger<Consumer32> _logger;

    public Consumer32(ILogger<Consumer32> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Input3> context)
    {
        _logger.LogInformation("32");
        await context.RespondAsync(new Output3 { Info = "Foo" });
    }
}
