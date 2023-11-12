using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
            MonitorService.Log.Here().Debug("We are in birthday method");
            if(birthday == null)
            {
                ViewBag.Error = "No date inserted";
                return View("Index");
            }

            var client = _clientFactory.CreateClient("MyClient");
            var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);
            var propagator = new TraceContextPropagator();

            var formattedDate = birthday.ToString("yyyy-MM-dd");
            MonitorService.Log.Here().Debug("Birthday date is {Date}", formattedDate);


            var request = new HttpRequestMessage(HttpMethod.Get, $"http://birthday-collector/BirthdayCollector?birthday={formattedDate}");
            propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

            var response = await client.SendAsync(request);
            MonitorService.Log.Here().Debug("resp returned {ret}", response.IsSuccessStatusCode);
            var result = await response.Content.ReadAsStringAsync();
            MonitorService.Log.Here().Debug("result is {res}", result);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = result;
                return View("Index");
            }
            else
            {
                MonitorService.Log.Here().Error(response.)
            }

            return View("Index");
        }
    }
}

