using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorBenchmark;

public class MeConsumer : IRequestHandler<SampleCommand, SampleResponse>
{
    public Task<SampleResponse> Handle(SampleCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SampleResponse($"R-Response for {request.Input}"));
    }
}

public class MediatRMediatorRunner
{
    private readonly IMediator _mediator;

    public MediatRMediatorRunner()
    {
        IServiceProvider sp = new ServiceCollection()
            .AddMediatR(Assembly.GetCallingAssembly()).BuildServiceProvider();
        _mediator = sp.GetService<IMediator>();
    }

    public async Task<SampleResponse> Run()
    {
        return await _mediator.Send(new SampleCommand("Hi"));
    }
}