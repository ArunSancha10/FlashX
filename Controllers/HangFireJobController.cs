using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using outofoffice.Repositories.Interfaces;
using System.Security.Principal;

namespace outofoffice.Controllers
{
    [Route("jobs")]
    public class HangFireJobController(IHangfireJobHandler hangfireJobHandler) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpDelete("delete-jobs/{AppId:guid}")]
        public async Task<IActionResult> DeleteJobs(Guid AppId)
        {
            try
            {
                var result = await hangfireJobHandler.DeleteJob(AppId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete("delete/{AppId:guid}")]
        public async Task<IActionResult> DeleteJob(Guid AppId , string name)
        {
            try
            {
                await hangfireJobHandler.DeleteSingleJob(AppId, name);

                return Ok(new { message = "Job Deleted Successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
