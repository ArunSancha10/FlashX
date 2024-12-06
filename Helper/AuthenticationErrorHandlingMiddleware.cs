using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using outofoffice.Repositories.Interfaces;

namespace outofoffice.Helper
{
    public class AuthenticationErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationErrorHandlingMiddleware> _logger;
        private readonly IAppSettingsManager _appSettingsManager;


        public AuthenticationErrorHandlingMiddleware(RequestDelegate next, ILogger<AuthenticationErrorHandlingMiddleware> logger, IAppSettingsManager appSettingsManager)
        {
            _next = next;
            _logger = logger;
            _appSettingsManager = appSettingsManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                // Handle MsalUiRequiredException (e.g., admin consent or user consent required)
                _logger.LogError(ex, "MsalUiRequiredException caught. Triggering interactive consent.");

                // Implement logic to redirect to the consent page
                var consentUrl = await GenerateAdminConsentUrl();
                context.Response.Redirect(consentUrl);
            }
        }

        private async Task<string> GenerateAdminConsentUrl()
        {
            // Generate the URL for admin consent
            var tenantId = "common"; // Change this if you have a specific tenant ID
            var clientId = "af4874ab-ce34-4576-8ec5-868fb5b46e2c"; // Your app's Client ID
            string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl");

            var redirectUri = Uri.EscapeDataString($"{updatedHost}"); // Your app's redirect URI

            // This is the URL where the admin can grant consent to the required permissions
            return $"https://login.microsoftonline.com/{tenantId}/adminconsent?client_id={clientId}&redirect_uri={redirectUri}";
        }
    }
}
