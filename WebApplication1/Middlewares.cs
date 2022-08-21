using MassTransit;

namespace WebApplication1;

public class MyConsumeMiddleware<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<MyConsumeMiddleware<T>> _logger;

    public MyConsumeMiddleware(ILogger<MyConsumeMiddleware<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        _logger.LogDebug("REQUEST {Message} {ConversationId}", context.Message, context.ConversationId);
        try
        {
            await next.Send(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Consumer crashed {Message} {ConversationId}", context.Message, context.ConversationId);
            throw;
        }
    }

    public void Probe(ProbeContext context) { }
}


public class MySendMiddleware<T> : IFilter<SendContext<T>> where T : class
{
    private readonly ILogger<MySendMiddleware<T>> _logger;

    public MySendMiddleware(ILogger<MySendMiddleware<T>> logger)
    {
        _logger = logger;
    }

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        if (context.DestinationAddress?.LocalPath == "/response")
        {
            _logger.LogDebug("RESPONSE {Message} {ConversationId}", context.Message, context.ConversationId);
        }

        return next.Send(context);
    }

    public void Probe(ProbeContext context) { }
}
