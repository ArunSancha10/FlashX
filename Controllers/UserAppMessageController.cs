using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using outofoffice.Dto;
using outofoffice.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace outofoffice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAppMessageController : ControllerBase
    {
        private readonly IUserAppMessageRepository _userAppMessageRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor httpContextAccessor;


        public UserAppMessageController(IUserAppMessageRepository userAppMessageRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userAppMessageRepository = userAppMessageRepository;
            _mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }

        // GET: api/UserAppMessage
        [HttpGet]
        public async Task<IActionResult> GetAllUserAppMessages()
        {
            var userAppMessages = await _userAppMessageRepository.GetAllAsync();
            return Ok(userAppMessages);
        }

        // GET: api/UserAppMessage/{companyId}/{groupId}/{userId}
        [HttpGet("{companyId}/{groupId}/{userId}")]
        public async Task<IActionResult> GetUserAppMessageById(Guid companyId, string groupId, string userId)
        {
            var userAppMessage = await _userAppMessageRepository.GetByIdAsync(companyId, groupId, userId);
            if (userAppMessage == null)
                return NotFound();

            return Ok(userAppMessage);
        }

        // POST: api/UserAppMessage
        [HttpPost]
        public async Task<IActionResult> CreateUserAppMessage([FromBody] UserAppMessageDTO userAppMessageDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var uaid = await _userAppMessageRepository.AddAsync(userAppMessageDto);

                return Ok(uaid);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        // PUT: api/UserAppMessage/{companyId}/{groupId}/{UAID} // UAID is unique so use that for Update
        [HttpPut("{companyId}/{groupId}/{UAID}")]
        public async Task<IActionResult> UpdateUserAppMessage(Guid companyId, string groupId, Guid UAID, [FromBody] UserAppMessageDTO userAppMessageDto)
        {
            if (companyId != userAppMessageDto.Company_ID || groupId != userAppMessageDto.Group_ID || UAID != userAppMessageDto.UAID)
                return BadRequest();

            await _userAppMessageRepository.UpdateAsync(userAppMessageDto);
            return NoContent();
        }

        // DELETE: api/UserAppMessage/{companyId}/{groupId}/{userId}
        [HttpDelete("{companyId}/{groupId}/{userId}")]
        public async Task<IActionResult> DeleteUserAppMessage(Guid companyId, string groupId, string userId)
        {
            await _userAppMessageRepository.DeleteAsync(companyId, groupId, userId);
            return NoContent();
        }

        [HttpGet("Recent/{User_ID}")]
        public async Task<IActionResult> GetUserAppMessageByPublishStatus(string User_ID)
        {
            try
            {
                var userAppMessage = await _userAppMessageRepository.GetUserAppMessageByPublishStatusAsync(User_ID);

                if (userAppMessage == null)
                    return NotFound();

                return Ok(userAppMessage);
            }
            catch (Exception ex)
            {
                //_logger.LogError($"Error retrieving message for publishStatus {publishStatus}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ListOfScheduledItems/{User_ID}")]
        public async Task<IActionResult> GetListOfScheduledItems(string User_ID)
        {
            try
            {
                var currentDateTime = DateTime.Now; // Get the current datetime
                var userAppMessages = await _userAppMessageRepository.GetUserAppMessagesAsync(currentDateTime, User_ID);


                if (userAppMessages == null)
                    return NotFound();

                return Ok(userAppMessages);
            }
            catch (Exception ex)
            {
                //_logger.LogError($"Error retrieving message for publishStatus {publishStatus}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("History/{User_ID}")]
        public async Task<IActionResult> historyOfScheduledItems(string User_ID)
        {
            try
            {
                var currentDateTime = DateTime.Now; // Get the current datetime
                var userAppMessages = await _userAppMessageRepository.historyOfScheduledItemsAsync(currentDateTime, User_ID);


                if (userAppMessages == null)
                    return NotFound();

                return Ok(userAppMessages);
            }
            catch (Exception ex)
            {
                //_logger.LogError($"Error retrieving message for publishStatus {publishStatus}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
