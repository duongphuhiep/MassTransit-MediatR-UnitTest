using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Moq.AutoMock.MassTransit;

public static class AutoMockerMassTransit
{
    /// <summary>
    /// Start a harness to test a single consumer. The consumer instance will be created with AutoMocker.
    /// Then you can just use this AutoMocker instance to access to the consumer instance (mocker.Get) or any of its Mock dependencies (mocker.GetMock)
    /// The returned test harness is started, so it is ready to be used as in the [MassTransit Documentation](https://masstransit-project.com/usage/testing.html)
    /// </summary>
    /// <typeparam name="T">type of the Consumer</typeparam>
    /// <param name="mocker">the AutoMocker instance</param>
    /// <returns>MassTransit Test Harness</returns>
    public static async Task<ITestHarness> StartTestHarnessFor<T>(this AutoMocker mocker) where T : class, IConsumer
    {
        var consumerUnderTest = mocker.CreateInstance<T>();
        return await StartTestHarnessFor(consumerUnderTest);
    }

    /// <summary>
    /// Start a harness to test a single consumer instance.
    /// The returned test harness is started, so it is ready to be used as in the [MassTransit Documentation](https://masstransit-project.com/usage/testing.html)
    /// </summary>
    /// <typeparam name="T">type of the Consumer</typeparam>
    /// <param name="consumer">the Consumer Under Test instance</param>
    /// <returns>MassTransit Test Harness</returns>
    public static async Task<ITestHarness> StartTestHarnessFor<T>(T consumer) where T : class, IConsumer
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<T>();
            })
            .AddSingleton(consumer)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        return harness;
    }
}