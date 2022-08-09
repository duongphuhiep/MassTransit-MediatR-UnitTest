using MassTransit;

namespace App.Demo1;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly INotifier _notifier;
    public SubmitOrderConsumer(INotifier notifier, IStockChecker stockChecker, IFidelityUpdater fidelityUpdater, IPromotionManager promotionManager)
    {
        _notifier = notifier;
    }
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var input = context.Message;
        await _notifier.Notify("Order submitted");
        await context.RespondAsync(new OrderSubmitted($"STOCK1/{input.Id}.{input.CustomerName}"));
        await context.RespondAsync(new OrderSubmitted($"STOCK2/{input.Id}.{input.CustomerName}"));
        await context.RespondAsync(new OrderSubmitted($"STOCK3/{input.Id}.{input.CustomerName}"));
    }
}
