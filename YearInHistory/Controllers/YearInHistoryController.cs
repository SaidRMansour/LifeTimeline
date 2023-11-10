using Microsoft.AspNetCore.Mvc;
using Monitoring;

namespace YearInHistory.Controllers;

[ApiController]
[Route("[controller]")]
public class YearInHistoryController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;

    public YearInHistoryController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int year)
    {
        MonitorService.Log.Here().Debug("Starting to fetch historical events for year: {Year}", year);

        var client = _clientFactory.CreateClient("MyClient");
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.api-ninjas.com/v1/historicalevents?year={year}");
        request.Headers.Add("X-Api-Key", "dhpBPfwR/DIxXZ71rGxg+w==0igua2G0cL21Hexh");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            MonitorService.Log.Here().Warning("Failed to fetch data for year {Year}. Status: {StatusCode}, Reason: {Reason}", year, response.StatusCode, response.ReasonPhrase);
            return StatusCode((int)response.StatusCode, $"Error retrieving data: {response.ReasonPhrase}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        MonitorService.Log.Here().Information("Successfully fetched historical events for year {Year}", year);

        return Ok(responseContent);
    }

}

