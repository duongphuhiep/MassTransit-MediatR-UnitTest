using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class Consumer2Controller : ControllerBase
{
    private readonly IRequestClient<Input2> _requestClient;

    public Consumer2Controller(IRequestClient<Input2> requestClient)
    {
        _requestClient = requestClient;
    }

    [HttpGet(Name = "Consumer2")]
    public async Task<Output2> Get()
    {
        using var tx = TransactionScopeFactory.CreateNew();
        var response = await _requestClient.GetResponse<Output2>(new Input2 { Info = "Input2" });
        tx!.Complete();
        return response.Message;
    }
}