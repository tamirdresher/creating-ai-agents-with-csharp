using Microsoft.AspNetCore.Mvc;

namespace SKCodeAssistent.Server.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AgentChat()
    {
        return View();
    }
}