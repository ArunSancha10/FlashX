using outofoffice.Models;
using outofoffice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using outofoffice.Dto;
using outofoffice.App_code;

namespace outofoffice.Repositories.Implementations
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly OOODbContext _context;

        public CompanyRepository(OOODbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompanyDTO>> GetAllAsync()
        {
            return await _context.Set<Company>().Select(c => new CompanyDTO
            {
                Company_ID = c.Company_ID,
                Company_Nm = c.Company_Nm
            }).ToListAsync();
        }

        public async Task<CompanyDTO> GetByIdAsync(Guid id)
        {
            var company = await _context.Set<Company>().FindAsync(id);
            return company == null ? null : new CompanyDTO
            {
                Company_ID = company.Company_ID,
                Company_Nm = company.Company_Nm
            };
        }

        public async Task AddAsync(CompanyDTO companyDto)
        {
            var company = new Company
            {
                Company_ID = companyDto.Company_ID,
                Company_Nm = companyDto.Company_Nm
            };
            _context.Set<Company>().Add(company);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CompanyDTO companyDto)
        {
            var company = await _context.Set<Company>().FindAsync(companyDto.Company_ID);
            if (company != null)
            {
                company.Company_Nm = companyDto.Company_Nm;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var company = await _context.Set<Company>().FindAsync(id);
            if (company != null)
            {
                _context.Set<Company>().Remove(company);
                await _context.SaveChangesAsync();
            }
        }
    }
}
