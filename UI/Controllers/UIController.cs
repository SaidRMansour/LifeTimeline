using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebUserInterface.Models;

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
}

