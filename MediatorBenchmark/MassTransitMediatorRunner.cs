using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorBenchmark;

public class MtConsumer : IConsumer<SampleCommand>
{
    public async Task Consume(ConsumeContext<SampleCommand> context)
    {
        await Task.Run(() => { });
        await context.RespondAsync(new SampleResponse($"M-Response for {context.Message.Input}"));
    }
}

public class MassTransitMediatorRunner
{
    private readonly IMediator _mediator;
    private readonly IRequestClient<SampleCommand> _requestClient;
    private readonly IServiceProvider _sp;

    public MassTransitMediatorRunner()
    {
        _sp = new ServiceCollection()
            .AddMediator(cfg => { cfg.AddConsumer<MtConsumer>(); })
            .BuildServiceProvider();

        _mediator = _sp.GetRequiredService<IMediator>();
        _requestClient = _mediator.CreateRequestClient<SampleCommand>();
    }

    public async Task<SampleResponse> Run()
    {
        return (await _requestClient.GetResponse<SampleResponse>(new SampleCommand("Hi"))).Message;
    }
}