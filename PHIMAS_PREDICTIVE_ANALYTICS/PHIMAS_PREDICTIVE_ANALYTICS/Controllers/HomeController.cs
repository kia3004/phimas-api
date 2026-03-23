using Microsoft.AspNetCore.Mvc;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}