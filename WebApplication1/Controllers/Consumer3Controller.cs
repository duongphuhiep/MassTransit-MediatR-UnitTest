using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class Consumer3Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public Consumer3Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet(Name = "Consumer3")]
    public string Get()
    {
        _mediator.Publish(new Input3 { Info = "bar" });
        return "ok";
    }
}