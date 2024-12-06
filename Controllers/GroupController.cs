using Microsoft.AspNetCore.Mvc;
using outofoffice.Repositories.Interfaces;
using System.Threading.Tasks;
using outofoffice.Dto;

namespace outofoffice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController: ControllerBase
    {
        private readonly IGroupRepository _groupRepository;

        public GroupController(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        // GET: api/Group
        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _groupRepository.GetAllAsync();
            return Ok(groups);
        }

        // GET: api/Group/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(string id)
        {
            var group = await _groupRepository.GetByIdAsync(id);
            if (group == null)
                return NotFound();

            return Ok(group);
        }

        // POST: api/Group
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDTO groupDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _groupRepository.AddAsync(groupDto);
            return CreatedAtAction(nameof(GetGroupById), new { id = groupDto.Group_ID }, groupDto);
        }

        // PUT: api/Group/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] GroupDTO groupDto)
        {
            if (id != groupDto.Group_ID)
                return BadRequest();

            await _groupRepository.UpdateAsync(groupDto);
            return NoContent();
        }

        // DELETE: api/Group/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            await _groupRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
