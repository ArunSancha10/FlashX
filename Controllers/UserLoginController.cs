using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using outofoffice.Service;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace outofoffice.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly LdapService _ldapService; // Corrected the service class name

        public UserLoginController(LdapService ldapService) // Corrected to match class name
        {
            _ldapService = ldapService;
        }

        public IActionResult Login()
        {
            ViewData["loginError"] = TempData["loginError"];
            return View();
        }

        [HttpPost] // Added the HttpPost attribute for a login method
        public Task<IActionResult> LoginAuthenticationAsync(string username, string password)
        {
            try
            {
                JObject response = _ldapService.validateUserByBind(username, password);

                if (response["status"]?.ToString() == "Success")
                {
                    HttpContext.Session.SetString("UserEmail", response["email"]?.ToString().ToLower());

                    // Redirect to the Recent page
                    return Task.FromResult<IActionResult>(RedirectToAction("Recent", "Home"));
                }
                else
                {
                    TempData["loginError"] = "Invalid username or password.";
                    return Task.FromResult<IActionResult>(RedirectToAction("Login"));
                }
            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 status code for errors
                Debug.WriteLine($"Exception in LoginAuthentication method: {ex.Message}");
                return Task.FromResult<IActionResult>(StatusCode(500, "Internal server error occurred."));
            }
        }

        public IActionResult Logout()
        {
            try
            {
                // Clear the session to log out the user
                HttpContext.Session.Remove("UserEmail");

                // Redirect to the login page after logout
                return RedirectToAction("Login", "UserLogin");
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.WriteLine($"Error during logout: {ex.Message}");

                // You can redirect to an error page or show a message
                return RedirectToAction("Error", "Home");
            }
        }

    }
}
