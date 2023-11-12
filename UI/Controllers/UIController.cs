using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using SharedModels;

namespace WebUserInterface.Controllers;

public class UIController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public UIController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    // GET method for loading the Index view
    [HttpGet]
    public IActionResult Index()
    {
        return View();

    }

    [HttpPost]
    public async Task <IActionResult> GetBirthday(DateTime birthday)
    {
        using (var activity = MonitorService.ActivitySource.StartActivity())
        {
            if(birthday == null)
            {
                ViewBag.Error = "No date inserted";
                return View("Index");
            }

            // Prepares and sends an HTTP request to the BirthdayCollector service to calculate age.
            var client = _clientFactory.CreateClient("MyClient");
            var formattedDate = birthday.ToString("yyyy-MM-dd");
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://birthday-service/BirthdayCollector?birthday={formattedDate}");

            // Propagates the current trace context to the outgoing request.
            var propagator = new TraceContextPropagator();
            var activityContext = Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);
            propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Fetches historical events associated with the year of the given birthday.
                var requestGetEvent = new HttpRequestMessage(HttpMethod.Get, $"http://birthday-service/BirthdayCollector/GetEvent?year={birthday.ToString("yyyy")}");
                propagator.Inject(propagationContext, requestGetEvent.Headers, (headers, key, value) => headers.Add(key, value));
                var responseGetEvent = await client.SendAsync(requestGetEvent);

                if (responseGetEvent.IsSuccessStatusCode)
                {
                    var resultGetEvent = await responseGetEvent.Content.ReadAsStringAsync();
                    // Deserializes and stores the historical events to be displayed on the view.
                    ViewBag.HistoryEvent = JsonConvert.DeserializeObject<List<HistoricalEvent>>(resultGetEvent);
                }

                // Stores the result of the age calculation to be displayed on the view.
                ViewBag.Result = result;
                return View("Index");
            }
            ViewBag.Result = "Failure";
            return View("Index");
        }
    }
}

