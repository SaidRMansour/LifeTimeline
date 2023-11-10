using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedModels;
using Monitoring;

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
    public IActionResult Get([FromQuery] string birthday)
    {
        if (!DateTime.TryParse(birthday, out DateTime parsedBirthday))
        {
            MonitorService.Log.Here().Error("Birthday could not be parsed to a Datetime: {Birthdate}", birthday);
            return BadRequest("Invalid date format. Please use YYYY-MM-DD format.");
        }
        MonitorService.Log.Here().Debug("Birthday given: {Birthdate}", birthday);
        var today = DateTime.Today;
        var age = today.Year - parsedBirthday.Year;

        // Check if the current year's birthday has passed; if not, subtract one from age
        if (parsedBirthday > today.AddYears(-age)) age--;

        return Ok(age);
    }

    [HttpGet(Name = "GetEvent")]
    public async Task<IActionResult> GetEvent(int year)
    {
        // Validate year input
        if (year < 1000 || year > 9999)
        {
            MonitorService.Log.Here().Error("Invalid year input: {Year}. Year must be a 4-digit number.", year);
            return BadRequest("Year must be a 4-digit number.");
        }

        MonitorService.Log.Here().Debug("Fetching historical events for year: {Year}", year);

        var client = _clientFactory.CreateClient("MyClient");

        var request = new HttpRequestMessage(HttpMethod.Get, $"http://yearinhistory/YearInHistory?{year}");
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            MonitorService.Log.Here().Warning("Failed to retrieve data for year {Year}. Status: {StatusCode}, Reason: {Reason}", year, response.StatusCode, response.ReasonPhrase);
            return StatusCode((int)response.StatusCode, $"Error retrieving data: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadAsStringAsync();
        var historicalEvents = JsonConvert.DeserializeObject<List<HistoricalEvent>>(result);

        MonitorService.Log.Here().Information("Successfully retrieved historical events for year {Year}", year);

        return Ok(historicalEvents);
    }

}

