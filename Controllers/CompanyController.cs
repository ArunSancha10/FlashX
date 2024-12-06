using Microsoft.AspNetCore.Mvc;
using outofoffice.Dto;
using outofoffice.Repositories.Interfaces;


namespace outofoffice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        // GET: api/Company
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetCompanies()
        {
            return Ok(await _companyRepository.GetAllAsync());
        }

        // GET: api/Company/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDTO>> GetCompany(Guid id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        // POST: api/Company
        [HttpPost]
        public async Task<ActionResult> CreateCompany([FromBody] CompanyDTO companyDto)
        {
            await _companyRepository.AddAsync(companyDto);
            return CreatedAtAction(nameof(GetCompany), new { id = companyDto.Company_ID }, companyDto);
        }

        // PUT: api/Company/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyDTO companyDto)
        {
            if (id != companyDto.Company_ID) return BadRequest();
            await _companyRepository.UpdateAsync(companyDto);
            return NoContent();
        }

        // DELETE: api/Company/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            await _companyRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
