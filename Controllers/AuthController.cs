using Microsoft.AspNetCore.Mvc;
using outofoffice.Helper;
using outofoffice.Repositories.Implementations;

namespace outofoffice.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Login()
        {
            var state = Guid.NewGuid().ToString(); // Generate a random state value
            var url = await _authService.GetAuthorizationUrl(state);

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    redirect = Url.Action("Login", "Auth")
                });
            }

            return Redirect(url);
        }

        public async Task<IActionResult> Callback(string code, string state, string error )
        {
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest($"Error: {error}");
            }

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code is missing.");
            }

            // Exchange authorization code for access and refresh tokens
            var tokenResponse = await _authService.GetTokenAsync(code);

            // Save tokens in session
            var session = HttpContext.Session;
            session.SetString("AccessToken", tokenResponse.AccessToken);
            session.SetString("RefreshToken", tokenResponse.RefreshToken);
            session.Sets("TokenExpiry", DateTime.UtcNow.AddSeconds(int.Parse(tokenResponse.ExpiresIn)));

            return RedirectToAction("index", "profile");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
