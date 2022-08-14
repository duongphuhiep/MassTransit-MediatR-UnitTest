using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IRequestClient<Input2> _requestClient;

    public WeatherForecastController(IRequestClient<Input2> requestClient)
    {
        _requestClient = requestClient;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<Output2> Get()
    {
        var response = await _requestClient.GetResponse<Output2>(new Input2 {Info = "Ha"});
        return response.Message;
    }
}