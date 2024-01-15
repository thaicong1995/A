using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.View
{
    public class HomeController : Controller
    {
        [HttpGet("test")]
        public IActionResult Index()
        {
            return View("test");
        }
    }
}
