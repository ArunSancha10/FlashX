using outofoffice.Models;
using outofoffice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using outofoffice.Dto;
using outofoffice.App_code;
using AutoMapper;

namespace outofoffice.Repositories.Implementations
{
    public class UserAppMessageRepository : IUserAppMessageRepository
    {
        private readonly OOODbContext _context;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserAppMessageRepository(OOODbContext context, IHttpContextAccessor _httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<UserAppMessageDTO>> GetAllAsync()
        {
            return await _context.Set<UserAppMessage>().Select(u => new UserAppMessageDTO
            {
                Company_ID = u.Company_ID,
                Group_ID = u.Group_ID,
                User_ID = u.User_ID,
                Message_Type = u.Message_Type,
                OOO_From_Dt = u.OOO_From_Dt,
                OOO_To_Dt = u.OOO_To_Dt,
                Message_Txt = u.Message_Txt,
                Apps_To_Publish = u.Apps_To_Publish,
                Publish_Status = u.Publish_Status
            }).ToListAsync();
        }

        public async Task<UserAppMessageDTO> GetByIdAsync(Guid companyId, string groupId, string userId)
        {
            var userMessage = await _context.Set<UserAppMessage>()
                .FirstOrDefaultAsync(u => u.Company_ID == companyId && u.Group_ID == groupId && u.User_ID == userId);

            return userMessage == null ? null : new UserAppMessageDTO
            {
                Company_ID = userMessage.Company_ID,
                Group_ID = userMessage.Group_ID,
                User_ID = userMessage.User_ID,
                Message_Type = userMessage.Message_Type,
                OOO_From_Dt = userMessage.OOO_From_Dt,
                OOO_To_Dt = userMessage.OOO_To_Dt,
                Message_Txt = userMessage.Message_Txt,
                Apps_To_Publish = userMessage.Apps_To_Publish,
                Publish_Status = userMessage.Publish_Status
            };
        }

        public async Task<UserAppMessageCreatedDTO> AddAsync(UserAppMessageDTO userAppMessageDto)
        {
            var userMessage = new UserAppMessage
            {
                Company_ID = userAppMessageDto.Company_ID,
                Group_ID = userAppMessageDto.Group_ID,
                User_ID = userAppMessageDto.User_ID,
                Message_Type = userAppMessageDto.Message_Type,
                OOO_From_Dt = userAppMessageDto.OOO_From_Dt,
                OOO_To_Dt = userAppMessageDto.OOO_To_Dt,
                Message_Txt = userAppMessageDto.Message_Txt,
                Apps_To_Publish = userAppMessageDto.Apps_To_Publish,
                Publish_Status = userAppMessageDto.Publish_Status
            };
             _context.Set<UserAppMessage>().Add(userMessage);
            
            await _context.SaveChangesAsync();

            return new UserAppMessageCreatedDTO { UAID = userMessage.UAID };
        }

        public async Task UpdateAsync(UserAppMessageDTO userAppMessageDto)
        {
            var userMessage = await _context.Set<UserAppMessage>()
                .FirstOrDefaultAsync(u => u.Company_ID == userAppMessageDto.Company_ID && u.Group_ID == userAppMessageDto.Group_ID && u.UAID == userAppMessageDto.UAID);

            if (userMessage != null)
            {
                userMessage.Message_Type = userAppMessageDto.Message_Type;
                userMessage.OOO_From_Dt = userAppMessageDto.OOO_From_Dt;
                userMessage.OOO_To_Dt = userAppMessageDto.OOO_To_Dt;
                userMessage.Message_Txt = userAppMessageDto.Message_Txt;
                userMessage.Apps_To_Publish = userAppMessageDto.Apps_To_Publish;
                userMessage.Publish_Status = userAppMessageDto.Publish_Status;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid companyId, string groupId, string userId)
        {
            var userMessage = await _context.Set<UserAppMessage>()
                .FirstOrDefaultAsync(u => u.Company_ID == companyId && u.Group_ID == groupId && u.User_ID == userId);

            if (userMessage != null)
            {
                _context.Set<UserAppMessage>().Remove(userMessage);
                await _context.SaveChangesAsync();
            }
        }

        // Add the GetRecentByPublishStatusAsync method here
        public async Task<UserAppMessage> GetUserAppMessageByPublishStatusAsync(string User_ID)
        {
            try
            {

                bool containsAdmin = false;
                if (User_ID.Contains("admin"))
                {
                    containsAdmin = true;
                    Console.WriteLine("The usermail contains 'admin'.");
                }

                if (containsAdmin)
                {
                    var messages = await _context.UserAppMessages.Where(user => user.IsDeleted == false).OrderByDescending(u => u.CreatedDate)
                    .FirstOrDefaultAsync();
                    return messages;
                }
                else
                {
                    var messages = await _context.UserAppMessages.Where(user => user.User_ID == User_ID && user.IsDeleted == false ).OrderByDescending(u => u.CreatedDate)
                    .FirstOrDefaultAsync();
                    return messages;
                }

            }
            catch (Exception ex)
            {
                // Log the exception
                // Example logging, replace with your actual logging mechanism
                //Console.Error.WriteLine($"Error: {ex.Message}");
                return null; // or rethrow the exception based on your handling strategy
            }
        }

        // Add the recent Dropdown values
        public async Task<List<UserAppMessage>> GetUserAppMessagesAsync(DateTime currentDateTime, string User_ID)
        {
            try
            {
                bool containsAdmin = false;
                if (User_ID.Contains("admin"))
                {
                    containsAdmin = true;
                    Console.WriteLine("The usermail contains 'admin'.");
                }

                List<UserAppMessage> messages;

                if (containsAdmin)
                {
                    messages = await _context.UserAppMessages
                        .Where(u => u.OOO_From_Dt >= currentDateTime && u.IsDeleted == false)
                        .OrderByDescending(u => u.CreatedDate)
                        .ToListAsync();
                }
                else
                {
                    messages = await _context.UserAppMessages
                        .Where(u => u.User_ID == User_ID && u.OOO_From_Dt >= currentDateTime && u.IsDeleted == false)
                        .OrderByDescending(u => u.CreatedDate)
                        .ToListAsync();
                }

                return messages;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging library or console for simplicity)
                Console.WriteLine($"An error occurred while fetching messages: {ex.Message}");
                // Optionally, rethrow the exception or return an empty list
                return new List<UserAppMessage>();
            }
        }

        // Add the History values
        public async Task<List<UserAppMessage>> historyOfScheduledItemsAsync(DateTime currentDateTime, string User_ID)
        {
            try
            {
                bool containsAdmin = false;
                if (User_ID.Contains("admin"))
                {
                    containsAdmin = true;
                    Console.WriteLine("The usermail contains 'admin'.");
                }

                List<UserAppMessage> messages;

                if (containsAdmin)
                {
                    messages = await _context.UserAppMessages
                .Where(u => u.OOO_From_Dt <= currentDateTime && u.IsDeleted == false)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
                }
                else
                {
                    messages = await _context.UserAppMessages
                .Where(u => u.User_ID == User_ID && u.OOO_From_Dt <= currentDateTime && u.IsDeleted == false)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
                }

                return messages;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging library or console for simplicity)
                Console.WriteLine($"An error occurred while fetching messages: {ex.Message}");
                // Optionally, rethrow the exception or return an empty list
                return new List<UserAppMessage>();
            }
        }
    }
}
