using App.Demo1;
using App.Demo2;
using App.Models;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
using Moq.AutoMock.MassTransit;
using ToolsPack.String;
using Xunit.Abstractions;

namespace App.Tests.Demo2;

public class ConsumersTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ConsumersTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task MultiResponseType()
    {
        // ARRANGE
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<Consumer1>();
                //cfg.AddConsumer<Consumer2>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        
        var client = harness.GetRequestClient<Input1>();
        var sampleInput = new Input1(1, "Hiep");

        // ACT

        var response1 = await client.GetResponse<Output1>(sampleInput);
        //var response2 = await client.GetResponse<Output2>(sampleInput);
        
        // ASSERT
        
        _testOutputHelper.WriteLine($"Total Consumed = {harness.Consumed}");
        _testOutputHelper.WriteLine($"Total Consumed = {harness.Consumed.Count()}");
        _testOutputHelper.WriteLine($"Total Sent (Output) = {harness.Sent.Count()}");
        
        var consumer1Harness = harness.GetConsumerHarness<Consumer1>();
        _testOutputHelper.WriteLine($"Consumer1. Total Consumed (Input+Output) = {consumer1Harness.Consumed.Count()}");
        
        _testOutputHelper.WriteLine($"response1 = {response1.Message}");
        //_testOutputHelper.WriteLine($"response2 = {response2.Message}");
    }

    public static string ToString(IReceivedMessageList list)
    {
        var ll = list.Select(x => x.MessageObject!=null);
        var messages = ll.Select(x => x.MessageObject);
        return messages.Display().ToString();
    }
    public static string ToString(ISentMessageList list)
    {
        var ll = list.Select(x => x.MessageObject!=null);
        var messages = ll.Select(x => x.MessageObject);
        return messages.Display().ToString();
    }
}