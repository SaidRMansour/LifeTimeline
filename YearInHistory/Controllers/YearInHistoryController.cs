using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
        // Extract telemetry context from the incoming request.
        var propagator = new TraceContextPropagator();
        var contextToInject = HttpContext.Request.Headers;
        var parentContext = propagator.Extract(default, contextToInject, (headers, key) =>
        {
            return headers.ContainsKey(key) ? new List<string> { headers[key].ToString() } : new List<string>();
        });

        Baggage.Current = parentContext.Baggage;

        using (var activity = MonitorService.ActivitySource.StartActivity("Get Add service received", ActivityKind.Consumer, parentContext.ActivityContext))
        {
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

}

