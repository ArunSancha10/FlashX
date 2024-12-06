using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using outofoffice.Models;

namespace outofoffice.Controllers
{
    public class GraphController : Controller
    {
        protected readonly GraphServiceClient _graphClient;
        protected readonly ITokenAcquisition _tokenAcquisition;
        protected readonly ILogger<HomeController> _logger;


        public GraphController(GraphServiceClient graphClient, ITokenAcquisition tokenAcquisition, ILogger<HomeController> logger)
        {
            _graphClient = graphClient;
            _tokenAcquisition = tokenAcquisition;
            _logger = logger;
        }

        // Gets a Graph client configured with
        // the specified scopes
        /*
        protected GraphServiceClient GetGraphClientForScopes(string[] scopes)
        {
            return GraphServiceClientFactory
                .GetAuthenticatedGraphClient(async () =>
                {
                    var token = await _tokenAcquisition
                        .GetAccessTokenForUserAsync(scopes);

                    // Uncomment to print access token to debug console
                    // This will happen for every Graph call, so leave this
                    // out unless you're debugging your token
                    //_logger.LogInformation($"Access token: {token}");

                    return token;
                }
            );
        }
        */

        protected async Task EnsureScopes(string[] scopes)
        {
            await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        }

        protected void InvokeAuthIfNeeded(ServiceException serviceException)
        {
            // Check if this failed because interactive auth is needed
            if (serviceException.InnerException is MicrosoftIdentityWebChallengeUserException)
            {
                // Throwing the exception causes Microsoft.Identity.Web to
                // take over, handling auth (based on scopes defined in the
                // AuthorizeForScopes attribute)
                throw serviceException;
            }
        }



    }
}
