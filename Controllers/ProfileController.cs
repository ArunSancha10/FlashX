using Microsoft.AspNetCore.Mvc;
using outofoffice.Dto;
using outofoffice.Helper;
using outofoffice.Repositories.Implementations;

namespace outofoffice.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AuthService _authService;

        public ProfileController(AuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var accessToken = await _authService.GetAccessTokenAsync();
                var userProfile = await _authService.CallMicrosoftGraphAsync(accessToken);
                var refreshToken = HttpContext.Session.GetString("RefreshToken");
                HttpContext.Session.SetString("GraphUserId",userProfile.Id);
                HttpContext.Session.SetString("UserPrincipalName", userProfile.UserPrincipalName);
                HttpContext.Session.SetString("GraphMail", userProfile.Mail);

                var messageAppListDto = new MessageAppListDTO
                {
                    MAID = new Guid(),
                    Company_ID =  Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6"),
                    Group_ID = "IT",
                    App_Nm = "Microsoft",
                    App_Desc = "Microsoft",
                    App_Channels = "appChannels",
                    Access_Token_User_ID = accessToken,
                    Access_Token_Txt = refreshToken,
                    Publish_Immd_Flag = true,
                    UserEmail = userProfile.Mail,
                    UserID = userProfile.Id
                };

                await _authService.AddOrUpdateAsync(messageAppListDto);

                return RedirectToAction("New","Home");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
        }
    }
}
