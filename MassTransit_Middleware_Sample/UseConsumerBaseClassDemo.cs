using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace Masstr.BaseClass;

#region Generic codes

public interface IRequest<out TResponse> where TResponse : BaseResponse, new()
{
}
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
    where TRequest : class, IRequest<TResponse>
    where TResponse : BaseResponse, new()
{
    private readonly ILogger<TRequest> _logger;

    protected BaseRequestResponseConsumer(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TRequest> context)
    {
        TRequest request = context.Message;
        _logger.LogInformation("REQUEST {Request} {ConversationId}", request, context.ConversationId);
        try
        {
            TResponse response = await Consume(request, context.CancellationToken);
            _logger.LogInformation("RESPONSE {Response} {ConversationId}", response, context.ConversationId);
            await context.RespondAsync(response);
        }
        catch (BusinessException businessEx)
        {
            var response = new TResponse()
            {
                ErrorMessage = businessEx.Message
            };
            _logger.LogWarning(businessEx, "RESPONSE Business Error {ConversationId}", context.ConversationId);
            await context.RespondAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRASHED while consuming {Request} {ConversationId}", request, context.ConversationId);
            throw;
        }
    }

    public abstract Task<TResponse> Consume(TRequest request, CancellationToken cancellationToken);
}

#endregion

public record MyRequest : IRequest<MyResponse> { }
public record MyResponse : BaseResponse { }
public interface ISomeDependentService { }

public class MyConsumer : BaseRequestResponseConsumer<MyRequest, MyResponse>
{
    public MyConsumer(ILogger<MyRequest> logger, ISomeDependentService someDependentService) : base(logger)
    {
    }

    public override Task<MyResponse> Consume(MyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new MyResponse());
    }
}

public class UseConsumerBaseClassDemo
{
    private readonly IServiceProvider _provider;
    private readonly ITestOutputHelper _testOutput;

    public UseConsumerBaseClassDemo(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        _provider = new ServiceCollection()
            .AddMediator(cfg =>
            {
                cfg.AddConsumer<MyConsumer>();
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
