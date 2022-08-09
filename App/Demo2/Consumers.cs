using App.Models;
using MassTransit;

namespace App.Demo2;

public class Consumer1: IConsumer<Input1>
{
    public async Task Consume(ConsumeContext<Input1> context)
    {
        await context.RespondAsync(new Output1("je suis output1 / response A"));
        await context.RespondAsync(new Output1("je suis output1 / response B"));
        await context.RespondAsync(new Output1("je suis output1 / response C"));
        await context.RespondAsync(new Output2("je suis output2 / response A"));
        await context.RespondAsync(new Output2("je suis output2 / response B"));
        await context.RespondAsync(new Output2("je suis output2 / response C"));
    }
}

public class Consumer2: IConsumer<Input2>
{
    public async Task Consume(ConsumeContext<Input2> context)
    {
        await context.RespondAsync(new Output2("foo"));
    }
}