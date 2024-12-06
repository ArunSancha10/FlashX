using outofoffice.Dto;
using outofoffice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace outofoffice.Repositories.Interfaces
{
    public interface IUserAppMessageRepository
    {
        Task<IEnumerable<UserAppMessageDTO>> GetAllAsync();
        Task<UserAppMessageDTO> GetByIdAsync(Guid companyId, string groupId, string userId);
        Task<UserAppMessageCreatedDTO> AddAsync(UserAppMessageDTO userAppMessageDto);
        Task UpdateAsync(UserAppMessageDTO userAppMessageDto);
        Task DeleteAsync(Guid companyId, string groupId, string userId);
        Task<UserAppMessage> GetUserAppMessageByPublishStatusAsync(string User_ID);
        Task<List<UserAppMessage>> GetUserAppMessagesAsync(DateTime currentDateTime, string User_ID);
        Task<List<UserAppMessage>> historyOfScheduledItemsAsync(DateTime currentDateTime, string User_ID);
    }
}
