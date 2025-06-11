using Microsoft.AspNetCore.Mvc;

namespace CollegeEventManager.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect to EventController's Index action
            return RedirectToAction("Index", "Event");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}