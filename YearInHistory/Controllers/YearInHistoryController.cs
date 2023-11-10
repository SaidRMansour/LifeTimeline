using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetAsync([FromQuery]int year)
    {
        var client = _clientFactory.CreateClient("MyClient");
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.api-ninjas.com/v1/historicalevents?year={year}");
        request.Headers.Add("X-Api-Key", "dhpBPfwR/DIxXZ71rGxg+w==0igua2G0cL21Hexh");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            // Handle the error, log it, and/or return an appropriate response
            return StatusCode((int)response.StatusCode, $"Error retrieving data: {response.ReasonPhrase}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return Ok(responseContent);
    }
}

