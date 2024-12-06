using outofoffice.Dto;
using outofoffice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace outofoffice.Repositories.Interfaces
{
    public interface IGroupRepository
    {
        Task<IEnumerable<GroupDTO>> GetAllAsync();
        Task<GroupDTO> GetByIdAsync(string id);
        Task AddAsync(GroupDTO groupDto);
        Task UpdateAsync(GroupDTO groupDto);
        Task DeleteAsync(string id);
    }
}
