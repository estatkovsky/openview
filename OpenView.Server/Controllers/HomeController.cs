using Microsoft.AspNetCore.Mvc;

namespace OpenView.Server.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
