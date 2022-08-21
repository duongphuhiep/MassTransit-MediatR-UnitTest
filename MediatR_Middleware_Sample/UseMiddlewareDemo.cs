using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Reflection;
using Xunit.Abstractions;

namespace MediatR.MiddlewareDemo;

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

public class LogTryCatchMiddleware<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : BaseResponse, new()
{
    private readonly ILogger<TRequest> _logger;

    public LogTryCatchMiddleware(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var conversationId = Guid.NewGuid();
        _logger.LogInformation("REQUEST {Request} {ConversationId}", request, conversationId);
        try
        {
            TResponse response = await next();
            _logger.LogInformation("RESPONSE {Response} {ConversationId}", response, conversationId);
            return response;
        }
        catch (BusinessException businessEx)
        {
            var response = new TResponse()
            {
                ErrorMessage = businessEx.Message
            };
            _logger.LogWarning(businessEx, "RESPONSE Business Error {ConversationId}", conversationId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRASHED while consuming {Request} {ConversationId}", request, conversationId);
            throw;
        }
    }
}

public record MyRequest : IRequest<MyResponse> { }
public record MyResponse : BaseResponse { }
public interface ISomeDependentService { }

public class MyConsumer : IRequestHandler<MyRequest, MyResponse>
{
    public MyConsumer(ISomeDependentService someDependentService)
    {
    }

    public Task<MyResponse> Handle(MyRequest request, CancellationToken cancellationToken)
    {
        throw new BusinessException("Business error");
        return Task.FromResult(new MyResponse());
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
            .AddMediatR(Assembly.GetExecutingAssembly())
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LogTryCatchMiddleware<,>))
            .AddLogging(cfg =>
            {
                cfg.AddXUnit(testOutput);
            })
            .AddSingleton(Substitute.For<ISomeDependentService>())
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    [Fact]
    public async Task MyConsumerTestAsync()
    {
        var mediator = _provider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new MyRequest());
        _testOutput.WriteLine($"Sender side successfully got a response: {response}");
    }
}
