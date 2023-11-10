using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedModels;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

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
    }

    [HttpGet(Name = "GetEvent")]
    public async Task<IActionResult> GetEvent(int year)
    {
        var propagator = new TraceContextPropagator();
        var activityContext = Activity.Current?.Context ?? default;
        var propagationContext = new PropagationContext(activityContext, Baggage.Current);

        // Validate year input
        if (year < 1000 || year > 9999)
        {
            MonitorService.Log.Here().Error("Invalid year input: {Year}. Year must be a 4-digit number.", year);
            return BadRequest("Year must be a 4-digit number.");
        }

        MonitorService.Log.Here().Debug("Fetching historical events for year: {Year}", year);

        var client = _clientFactory.CreateClient("MyClient");

        var request = new HttpRequestMessage(HttpMethod.Get, $"http://yearinhistory/YearInHistory?{year}");

        propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

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

