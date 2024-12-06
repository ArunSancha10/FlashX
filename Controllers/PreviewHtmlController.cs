using Microsoft.AspNetCore.Mvc;
using outofoffice.Models;

namespace outofoffice.Controllers
{
    public class PreviewHtmlController : Controller
    {
        public IActionResult slackPreview()
        {
            ViewData["ShowPreviewHeaderFooter"] = false;
            return View();
        }
        public IActionResult teamsPreview()
        {
            ViewData["ShowPreviewHeaderFooter"] = false;
            return View();
        }
        public IActionResult zohoPreview()
        {
            ViewData["ShowPreviewHeaderFooter"] = false;
            return View();
        }
        public IActionResult outlookPreview()
        {
            ViewData["ShowPreviewHeaderFooter"] = false;
            return PartialView();
        }
        public IActionResult sharepointPreview()
        {
            ViewData["ShowPreviewHeaderFooter"] = false;
            return View();
        }
    }
}
