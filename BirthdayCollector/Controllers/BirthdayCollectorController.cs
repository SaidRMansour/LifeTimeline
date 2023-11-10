using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollector.Controllers;

[ApiController]
[Route("[controller]")]
public class BirthdayCollectorController : ControllerBase
{

    [HttpGet(Name = "GetWeatherForecast")]
    public string Get()
    {
        Console.WriteLine("Test Github");
        return "Hello World";
    }
}

