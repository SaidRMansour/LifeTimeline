using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;


namespace YearInHistory.Controllers;

    [ApiController]
    [Route("[controller]")]
    public class YearInHistoryController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;

        // Circuit breaker
        private static AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy;

        public YearInHistoryController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromMinutes(1)
                );
    }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] string year)
        {
            // Extract telemetry context from the incoming request.
            var propagator = new TraceContextPropagator();
            var contextToInject = HttpContext.Request.Headers;
            var parentContext = propagator.Extract(default, contextToInject, (headers, key) =>
            {
                return headers.ContainsKey(key) ? new List<string> { headers[key].ToString() } : new List<string>();
            });

            Baggage.Current = parentContext.Baggage;

            using (var activity = MonitorService.ActivitySource.StartActivity("Get Event service received", ActivityKind.Consumer, parentContext.ActivityContext))
            {

                // Validate year input.
                if (!int.TryParse(year, out int parsedYear) || parsedYear < 1000 || parsedYear > 9999)
                {
                    MonitorService.Log.Here().Error("Invalid year input: {Year}. Year must be a 4-digit number.", year);
                    return BadRequest("Year must be a 4-digit number.");
                }
                // Create a new client and send a request to the external historical events API.
                var client = _clientFactory.CreateClient("MyClient");
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.api-ninjas.com/v1/historicalevents?year={year}");
                request.Headers.Add("X-Api-Key", "dhpBPfwR/DIxXZ71rGxg+w==0igua2G0cL21Hexh");

                // NEW
                try
                {
                    var response = await circuitBreakerPolicy.ExecuteAsync(() =>
                        client.SendAsync(request)
                    );

                    // Check if the response is successful.
                    if (!response.IsSuccessStatusCode)
                    {
                        MonitorService.Log.Here().Warning("Failed to fetch data for year {Year}. Status: {StatusCode}, Reason: {ReasonPhrase}.", year, response.StatusCode, response.ReasonPhrase);
                        return StatusCode((int)response.StatusCode, $"Error retrieving data: {response.ReasonPhrase}");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Ok(responseContent);
                }
                catch (BrokenCircuitException)
                {
                    
                    return StatusCode(503, "Service Temporarily Unavailable");
                }
                catch (Exception ex)
                {
                    
                    return StatusCode(500, "Internal Server Error");
                }



            
        }

    }
}

