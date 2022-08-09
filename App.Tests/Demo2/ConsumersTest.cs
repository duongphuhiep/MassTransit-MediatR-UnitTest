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
                cfg.AddConsumer<Consumer11>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        
        var client = harness.GetRequestClient<Input1>();

        // ACT

        var response1 = await client.GetResponse<Output1>(new Input1(1, "Hiep"));
        var response2 = await client.GetResponse<Output2>(new Input1(2, "Nhu"));
        
        // ASSERT
        
        _testOutputHelper.WriteLine($"Total Consumed = {harness.Consumed.Count()}");
        _testOutputHelper.WriteLine(ToString(harness.Consumed));
        
        _testOutputHelper.WriteLine($"Total Sent = {harness.Sent.Count()}");
        _testOutputHelper.WriteLine(ToString(harness.Sent));
        
        var consumer1Harness = harness.GetConsumerHarness<Consumer11>();
        _testOutputHelper.WriteLine($"Consumer1: Consumed Count = {consumer1Harness.Consumed.Count()}");
        _testOutputHelper.WriteLine(ToString(consumer1Harness.Consumed));
        
        _testOutputHelper.WriteLine($"response1 = {response1.Message}");
        _testOutputHelper.WriteLine($"response2 = {response2.Message}");
    }

    [Fact]
    public async Task Consumer21_Call_Cosumer12()
    {
        // ARRANGE
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<Consumer21>();
                cfg.AddConsumer<Consumer12>();
            })
            .BuildServiceProvider(new ServiceProviderOptions() {ValidateScopes = true, ValidateOnBuild = true});

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        

        // ACT

        var client = harness.GetRequestClient<Input2>();
        var response = await client.GetResponse<Output2>(new Input2(1, "Hiep"));
        
        // ASSERT
        
        _testOutputHelper.WriteLine($"response= {response.Message}");
        
        var consumer12Harness = harness.GetConsumerHarness<Consumer12>();
        _testOutputHelper.WriteLine($"Consumer12: Consumed Count = {consumer12Harness.Consumed.Count()}");
        _testOutputHelper.WriteLine(ToString(consumer12Harness.Consumed));
        
        var consumer21Harness = harness.GetConsumerHarness<Consumer12>();
        _testOutputHelper.WriteLine($"Consumer21: Consumed Count = {consumer21Harness.Consumed.Count()}");
        _testOutputHelper.WriteLine(ToString(consumer21Harness.Consumed));
        
        
        _testOutputHelper.WriteLine($"Total Consumed = {harness.Consumed.Count()}");
        _testOutputHelper.WriteLine(ToString(harness.Consumed));
        
        _testOutputHelper.WriteLine($"Total Sent = {harness.Sent.Count()}");
        _testOutputHelper.WriteLine(ToString(harness.Sent));
    }
    
    public static string ToString(IReceivedMessageList list)
    {
        var ll = list.Select(x => x.MessageObject!=null);
        var messages = ll.Select(x => x.MessageObject);
        return messages.Display().SeparatedByNewLine().ToString();
    }
    public static string ToString(ISentMessageList list)
    {
        var ll = list.Select(x => x.MessageObject!=null);
        var messages = ll.Select(x => x.MessageObject);
        return messages.Display().SeparatedByNewLine().ToString();
    }
}