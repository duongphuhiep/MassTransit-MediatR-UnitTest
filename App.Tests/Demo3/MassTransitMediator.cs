using App.Models;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests.Demo3;

public class MtConsumer : IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await context.RespondAsync(new Output1());
    }
}

public class MassTransitMediator
{
    private IServiceProvider _sp;
    private IMediator _mediator;
    
    public MassTransitMediator()
    {
        _sp = new ServiceCollection()
            .AddMediator(cfg =>
            {
                cfg.AddConsumer<MtConsumer>();
            })
            .BuildServiceProvider();

        _mediator = _sp.GetRequiredService<IMediator>();
    }

    [Fact]
    public void Run()
    {
        var requestClient = _mediator.CreateRequestClient<Input1>();
        var response =  requestClient.GetResponse<Output1>(new Input1());
        Assert.NotNull(response);
    }
}