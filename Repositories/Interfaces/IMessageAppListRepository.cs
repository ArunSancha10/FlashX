using outofoffice.Dto;
using outofoffice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace outofoffice.Repositories.Interfaces
{
    public interface IMessageAppListRepository
    {
        Task<IEnumerable<MessageAppListDTO>> GetAllAsync();
        Task<MessageAppListDTO> GetByIdAsync(Guid companyId, string groupId);
        Task AddAsync(MessageAppListDTO messageAppListDto);
        Task UpdateAsync(MessageAppListDTO messageAppListDto);
        Task DeleteAsync(Guid companyId, string groupId);
    }
}
