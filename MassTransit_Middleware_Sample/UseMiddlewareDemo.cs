using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Reflection;
using Xunit.Abstractions;

namespace Masstr.Middleware;

#region Generic codes

public interface IRequest<out TResponse> { }

public record BaseResponse
{
    public string? ErrorMessage { get; init; }
}

public class BusinessException : Exception
{
    public BusinessException(string? message) : base(message)
    {
    }
}

public abstract class BaseRequestResponseConsumer<TRequest, TResponse> : IConsumer<TRequest>
    where TRequest : class
    where TResponse : BaseResponse, new()
{
    public async Task Consume(ConsumeContext<TRequest> context)
    {
        TResponse response = await Consume(context.Message, context.CancellationToken);
        await context.RespondAsync(response);
    }

    public abstract Task<TResponse> Consume(TRequest request, CancellationToken cancellationToken);
}

public class LogResponseSendFilter<T> : IFilter<SendContext<T>> where T : class
{
    private readonly ILogger<T> _logger;

    public LogResponseSendFilter(ILogger<T> logger)
    {
        _logger = logger;
    }

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        if (context.DestinationAddress?.LocalPath == "/response")
        {
            _logger.LogInformation("RESPONSE {Response} {ConversationId}", context.Message, context.ConversationId);
        }

        return next.Send(context);
    }

    public void Probe(ProbeContext context) { }
}

public class LogTryCatchConsumeFilter<TRequest> : IFilter<ConsumeContext<TRequest>> where TRequest : class
{
    private readonly ILogger<TRequest> _logger;

    public LogTryCatchConsumeFilter(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<TRequest> context, IPipe<ConsumeContext<TRequest>> next)
    {
        _logger.LogInformation("REQUEST {Request} {ConversationId}", context.Message, context.ConversationId);
        try
        {
            await next.Send(context);
        }
        catch (BusinessException businessEx)
        {
            _logger.LogWarning(businessEx, "RESPONSE Business Error {ConversationId}", context.ConversationId);
            var errorResponse = TryBuildErrorResponse(businessEx);
            if (errorResponse is not null)
            {
                await context.RespondAsync(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRASHED while consuming {Request} {ConversationId}", context.Message, context.ConversationId);
            throw;
        }
    }

    /// <summary>
    /// Find the TResponse from the TRequest and try to instanciate it with the businessEx. Return null if
    /// (1) unable to find the TResponse (the TRequest not implement the IRequest).
    /// (2) the TResponse is not a BaseResponse.
    /// (3) unable to Instanciate the TResponse (no default constructor).
    /// </summary>
    /// <param name="tse">Business Logic Exception need to convert to the TResponse</param>
    /// <returns>null if TResponse is not a `BaseResponse` or do not have default constructor</returns>
    private static BaseResponse? TryBuildErrorResponse(BusinessException businessEx)
    {
        Type? responseType = GetResponseType();
        if (responseType == null || !responseType.IsSubclassOf(typeof(BaseResponse)) || !HasDefaultConstructor(responseType))
        {
            return null;
        }
        return (BaseResponse)Activator.CreateInstance(responseType)! with
        {
            ErrorMessage = businessEx.Message
        };
    }

    /// <summary>
    /// The TRequest implement the interface IRequest<TResponse>. This function return the TReponse from the TRequest.
    /// </summary>
    /// <returns>null if the TRequest does not implement IRequest</returns>
    private static Type? GetResponseType()
    {
        var requestType = (TypeInfo)typeof(TRequest);
        var requestInterface = requestType.ImplementedInterfaces.FirstOrDefault(
            p => p.FullName != null
            && p.FullName.StartsWith(typeof(IRequest<>).FullName!)
        );
        var responseType = requestInterface?.GenericTypeArguments.FirstOrDefault();
        return responseType;
    }

    /// <summary>
    /// Check if there is a default constructor
    /// </summary>
    private static bool HasDefaultConstructor(Type type)
    {
        return type.GetConstructors().Any(t => !t.GetParameters().Any());
    }

    public void Probe(ProbeContext context) { }
}

#endregion

public record MyRequest : IRequest<MyResponse> { }
public record MyResponse : BaseResponse { }
public interface ISomeDependentService { }

public class MyConsumer : BaseRequestResponseConsumer<MyRequest, MyResponse>
{
    public MyConsumer(ISomeDependentService someDependentService)
    {
    }

    public override Task<MyResponse> Consume(MyRequest request, CancellationToken cancellationToken)
    {
        throw new BusinessException("Business error");
        //return Task.FromResult(new MyResponse());
    }
}

public class UseMiddlewareDemo
{
    private readonly IServiceProvider _provider;
    private readonly ITestOutputHelper _testOutput;

    public UseMiddlewareDemo(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        _provider = new ServiceCollection()
            .AddMediator(cfg =>
            {
                cfg.AddConsumer<MyConsumer>();
                cfg.ConfigureMediator((context, mcfg) =>
                {
                    mcfg.UseConsumeFilter(typeof(LogTryCatchConsumeFilter<>), context);
                    mcfg.UseSendFilter(typeof(LogResponseSendFilter<>), context);
                });
            })
            .AddLogging(cfg =>
            {
                cfg.AddFilter("MassTransit", LogLevel.None); //disable MassTransit logs
                cfg.AddXUnit(testOutput);
            })
            .AddSingleton(Substitute.For<ISomeDependentService>())
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    [Fact]
    public async Task MyConsumerTestAsync()
    {
        var mediator = _provider.GetRequiredService<IMediator>();
        var client = mediator.CreateRequestClient<MyRequest>();
        var response = await client.GetResponse<MyResponse>(new MyRequest());
        _testOutput.WriteLine($"Sender side successfully got a response: {response.Message}");
    }
}
