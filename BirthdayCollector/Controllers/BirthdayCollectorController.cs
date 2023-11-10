using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedModels;

namespace BirthdayCollector.Controllers;

[ApiController]
[Route("[controller]")]
public class BirthdayCollectorController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;

    public BirthdayCollectorController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] DateTime birthday)
    {
        var today = DateTime.Today;
        var age = today.Year - birthday.Year;

        // Check if the current year's birthday has passed; if not, subtract one from age
        if (birthday > today.AddYears(-age)) age--;

        return Ok(age);
    }

    [HttpGet(Name = "GetEvent")]
    public async Task<IActionResult> GetEvent(int year)
    {
        // Validate year input
        if (year < 1000 || year > 9999)
        {
            return BadRequest("Year must be a 4-digit number.");
        }

        var client = _clientFactory.CreateClient("MyClient");

        var request = new HttpRequestMessage(HttpMethod.Get, $"http://year-in-history-service/YearInHistory?{year}");
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            // Handle the error, log it, and/or return an appropriate response
            return StatusCode((int)response.StatusCode, $"Error retrieving data: {response.ReasonPhrase}");
        }

        // Deserialiser JSON to a list of HistoricalEvent-objects
        var result = await response.Content.ReadAsStringAsync();
        var historicalEvents = JsonConvert.DeserializeObject<List<HistoricalEvent>>(result);

        return Ok(historicalEvents);
    }
}

