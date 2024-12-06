using Microsoft.AspNetCore.Mvc;
using outofoffice.Repositories.Interfaces;
using System;
using System.Threading.Tasks;
using outofoffice.Dto;

namespace outofoffice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageAppListController: ControllerBase
    {
        private readonly IMessageAppListRepository _messageAppListRepository;

        public MessageAppListController(IMessageAppListRepository messageAppListRepository)
        {
            _messageAppListRepository = messageAppListRepository;
        }

        // GET: api/MessageAppList
        [HttpGet]
        public async Task<IActionResult> GetAllMessageApps()
        {
            var messageApps = await _messageAppListRepository.GetAllAsync();
            return Ok(messageApps);
        }

        // GET: api/MessageAppList/{companyId}/{groupId}
        [HttpGet("{companyId}/{groupId}")]
        public async Task<IActionResult> GetMessageAppById(Guid companyId, string groupId)
        {
            var messageApp = await _messageAppListRepository.GetByIdAsync(companyId, groupId);
            if (messageApp == null)
                return NotFound();

            return Ok(messageApp);
        }

        // POST: api/MessageAppList
        //[Route("SaveTokens")]
        [HttpPost]
        public async Task<IActionResult> CreateMessageApp([FromBody] MessageAppListDTO messageAppListDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _messageAppListRepository.AddAsync(messageAppListDto);
            return CreatedAtAction(nameof(GetMessageAppById), new { companyId = messageAppListDto.Company_ID, groupId = messageAppListDto.Group_ID }, messageAppListDto);
        }
       

        // PUT: api/MessageAppList/{companyId}/{groupId}
        [HttpPut("{companyId}/{groupId}")]
        public async Task<IActionResult> UpdateMessageApp(Guid companyId, string groupId, [FromBody] MessageAppListDTO messageAppListDto)
        {
            if (companyId != messageAppListDto.Company_ID || groupId != messageAppListDto.Group_ID)
                return BadRequest();

            await _messageAppListRepository.UpdateAsync(messageAppListDto);
            return NoContent();
        }

        // DELETE: api/MessageAppList/{companyId}/{groupId}
        [HttpDelete("{companyId}/{groupId}")]
        public async Task<IActionResult> DeleteMessageApp(Guid companyId, string groupId)
        {
            await _messageAppListRepository.DeleteAsync(companyId, groupId);
            return NoContent();
        }
    }
}
