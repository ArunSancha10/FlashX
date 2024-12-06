using Microsoft.AspNetCore.Mvc;

namespace outofoffice.Controllers
{
    public class RightSideViewController : Controller
    {
        public IActionResult slack()
        {
            return View();
        }
    }
}
