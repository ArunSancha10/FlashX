using outofoffice.Models;
using outofoffice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using outofoffice.Dto;
using outofoffice.App_code;

namespace outofoffice.Repositories.Implementations
{
    public class MessageAppListRepository:  IMessageAppListRepository
    {
        private readonly OOODbContext _context;

        public MessageAppListRepository(OOODbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MessageAppListDTO>> GetAllAsync()
        {
            return await _context.Set<MessageAppList>().Select(m => new MessageAppListDTO
            {
                Company_ID = m.Company_ID,
                Group_ID = m.Group_ID,
                App_Nm = m.App_Nm,
                App_Desc = m.App_Desc,
                App_Channels = m.App_Channels,
                Access_Token_User_ID = m.Access_Token_User_ID,
                Access_Token_Txt = m.Access_Token_Txt,
                Publish_Immd_Flag = m.Publish_Immd_Flag
            }).ToListAsync();
        }

        public async Task<MessageAppListDTO> GetByIdAsync(Guid companyId, string groupId)
        {
            var messageApp = await _context.Set<MessageAppList>()
                .FirstOrDefaultAsync(m => m.Company_ID == companyId && m.Group_ID == groupId);

            return messageApp == null ? null : new MessageAppListDTO
            {
                Company_ID = messageApp.Company_ID,
                Group_ID = messageApp.Group_ID,
                App_Nm = messageApp.App_Nm,
                App_Desc = messageApp.App_Desc,
                App_Channels = messageApp.App_Channels,
                Access_Token_User_ID = messageApp.Access_Token_User_ID,
                Access_Token_Txt = messageApp.Access_Token_Txt,
                Publish_Immd_Flag = messageApp.Publish_Immd_Flag
            };
        }

        public async Task AddAsync(MessageAppListDTO messageAppListDto)
        {
            try
            {
                var messageApp = new MessageAppList
                {
                    MAID = messageAppListDto.MAID,
                    Company_ID = messageAppListDto.Company_ID,
                    Group_ID = messageAppListDto.Group_ID,
                    App_Nm = messageAppListDto.App_Nm,
                    App_Desc = messageAppListDto.App_Desc,
                    App_Channels = messageAppListDto.App_Channels,
                    Access_Token_User_ID = messageAppListDto.Access_Token_User_ID,
                    Access_Token_Txt = messageAppListDto.Access_Token_Txt,
                    Publish_Immd_Flag = messageAppListDto.Publish_Immd_Flag,
                    UserEmail = messageAppListDto.UserEmail,
                    UserID = messageAppListDto.UserID
                };
                _context.Set<MessageAppList>().Add(messageApp);
                //_context.MessageAppLists.AddAsync(messageApp);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
          
        }

        public async Task UpdateAsync(MessageAppListDTO messageAppListDto)
        {
            var messageApp = await _context.Set<MessageAppList>()
                .FirstOrDefaultAsync(m => m.UserID == messageAppListDto.UserID && m.UserEmail == messageAppListDto.UserEmail);

            if (messageApp != null)
            {
                messageApp.App_Nm = messageAppListDto.App_Nm;
                messageApp.App_Desc = messageAppListDto.App_Desc;
                messageApp.App_Channels = messageAppListDto.App_Channels;
                messageApp.Access_Token_User_ID = messageAppListDto.Access_Token_User_ID;
                messageApp.Access_Token_Txt = messageAppListDto.Access_Token_Txt;
                messageApp.Publish_Immd_Flag = messageAppListDto.Publish_Immd_Flag;
                messageApp.UserEmail = messageAppListDto.UserEmail;
                messageApp.UserID = messageAppListDto.UserID;

                await _context.SaveChangesAsync();
            }
            else
            {
                await AddAsync(messageAppListDto);
            }
        }

        public async Task DeleteAsync(Guid companyId, string groupId)
        {
            var messageApp = await _context.Set<MessageAppList>()
                .FirstOrDefaultAsync(m => m.Company_ID == companyId && m.Group_ID == groupId);

            if (messageApp != null)
            {
                _context.Set<MessageAppList>().Remove(messageApp);
                await _context.SaveChangesAsync();
            }
        }
    }
}
