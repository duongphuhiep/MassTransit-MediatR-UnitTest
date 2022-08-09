using App.Demo1;
using MassTransit.Testing;
using Moq;
using Moq.AutoMock;
using Moq.AutoMock.MassTransit;
using Xunit.Abstractions;

namespace App.Tests.Demo1;

public class SubmitOrderConsumerTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SubmitOrderConsumerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SimpleCase()
    {
        // ARRANGE

        AutoMocker mocker = new AutoMocker();
        var harness = await mocker.StartTestHarnessFor<SubmitOrderConsumer>();
        var client = harness.GetRequestClient<SubmitOrder>();
        var sampleInput = new SubmitOrder(1, "Hiep");

        // ACT

        var response = await client.GetResponse<OrderSubmitted>(sampleInput);
        
        // ASSERT
        
        _testOutputHelper.WriteLine(harness.Sent.Count().ToString());
        _testOutputHelper.WriteLine(response.Message.ToString());

        //that the sampleInput is consumed (there exist a SubmitOrder in the ReceivedMessageList)
        Assert.True(await harness.Consumed.Any<SubmitOrder>());
        Assert.True(await harness.Consumed.Any<SubmitOrder>(x => ((SubmitOrder)x.MessageObject).Id == sampleInput.Id));

        //that the SubmitOrderConsumer is invoked (in case there are multiple Consumer)
        var consumerHarness = harness.GetConsumerHarness<SubmitOrderConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<SubmitOrder>());
        Assert.True(await consumerHarness.Consumed.Any<SubmitOrder>(x => ((SubmitOrder)x.MessageObject).Id == sampleInput.Id));

        //that a response is sent back in the harness (there exist a OrderSubmitted in the SentMessageList)
        Assert.True(await harness.Sent.Any<OrderSubmitted>());
        Assert.True(await harness.Sent.Any<OrderSubmitted>(x => ((OrderSubmitted)x.MessageObject).SubmitReference.Contains(sampleInput.CustomerName)));

        //that the customer is notified "Order submitted"
        mocker.GetMock<INotifier>().Verify(m => m.Notify(It.Is<string>(v => v == "Order submitted")));

        //that the response contains the customer's name
        Assert.Contains(sampleInput.CustomerName, response.Message.SubmitReference);
    }
    
    
    
}