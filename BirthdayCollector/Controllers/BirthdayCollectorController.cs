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

        MonitorService.Log.Here().Debug("We are inside the birthday service");

        using (var activity = MonitorService.ActivitySource.StartActivity("Get Age method is called", ActivityKind.Consumer, parentContext.ActivityContext))
        {
            if (!DateTime.TryParse(birthday, out DateTime parsedBirthday) || parsedBirthday.Year < 1000 || parsedBirthday.Year > 9999)
            {
                MonitorService.Log.Here().Error("Invalid birthday format: {Birthday}", birthday);
                return BadRequest("Invalid date format. Please use YYYY-MM-DD format.");
            }
            var today = DateTime.Today;
            var age = today.Year - parsedBirthday.Year;

            // Check if the current year's birthday has passed; if not, subtract one from age
            if (parsedBirthday > today.AddYears(-age)) age--;

            return Ok(age);
        }
    }

    [HttpGet("GetEvent")]
    public async Task<IActionResult> GetEvent([FromQuery] string year)
    {
        // Extract telemetry context from the incoming request.
        var propagator = new TraceContextPropagator();
        var contextToInject = HttpContext.Request.Headers;
        var parentContext = propagator.Extract(default, contextToInject, (headers, key) =>
        {
            return headers.ContainsKey(key) ? new List<string> { headers[key].ToString() } : new List<string>();
        });
        Baggage.Current = parentContext.Baggage;

        using (var activity = MonitorService.ActivitySource.StartActivity("Get Event in Birthday service method is called", ActivityKind.Consumer, parentContext.ActivityContext))
        {
            var propagatorNew = new TraceContextPropagator();
            var activityContext = Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);

            MonitorService.Log.Here().Debug("We are inside the GetEvent method in Birthday service");

            // Validate the year input.
            if (!int.TryParse(year, out int newYear) || newYear < 1000 || newYear > 9999)
            {
                // Log error if the year is not a valid 4-digit number.
                MonitorService.Log.Here().Error("Invalid year input: {Year}", year);
                return BadRequest("Year must be a 4-digit number.");
            }

            // Create a new client and send a request to the historical events service.
            var client = _clientFactory.CreateClient("MyClient");
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://yearinhistory-service/YearInHistory?year={newYear}");
            propagatorNew.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));
            var response = await client.SendAsync(request);

            // Check the response status code and log a warning if the request was not successful.
            if (!response.IsSuccessStatusCode)
            {
                MonitorService.Log.Here().Warning("Failed to retrieve data for year {Year}", newYear);
                return StatusCode((int)response.StatusCode, $"Error retrieving data: {response.ReasonPhrase}");
            }

            // Deserialize the successful response into a list of historical events.
            var result = await response.Content.ReadAsStringAsync();
            var historicalEvents = JsonConvert.DeserializeObject<List<HistoricalEvent>>(result);
            return Ok(historicalEvents);
        }
    }

}

