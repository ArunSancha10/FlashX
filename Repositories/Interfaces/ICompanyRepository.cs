using outofoffice.Dto;
using outofoffice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace outofoffice.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<CompanyDTO>> GetAllAsync();
        Task<CompanyDTO> GetByIdAsync(Guid id);
        Task AddAsync(CompanyDTO companyDto);
        Task UpdateAsync(CompanyDTO companyDto);
        Task DeleteAsync(Guid id);
    }
}
