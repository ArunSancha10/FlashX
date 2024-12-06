using outofoffice.Models;
using outofoffice.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using outofoffice.Dto;
using outofoffice.App_code;


namespace outofoffice.Repositories.Implementations
{
    public class GroupRepository : IGroupRepository
    {
        private readonly OOODbContext _context;

        public GroupRepository(OOODbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GroupDTO>> GetAllAsync()
        {
            return await _context.Set<Group>().Select(g => new GroupDTO
            {
                Group_ID = g.Group_ID,
                Company_ID = g.Company_ID,
                Group_Nm = g.Group_Nm
            }).ToListAsync();
        }

        public async Task<GroupDTO> GetByIdAsync(string id)
        {
            var group = await _context.Set<Group>().FindAsync(id);
            return group == null ? null : new GroupDTO
            {
                Group_ID = group.Group_ID,
                Company_ID = group.Company_ID,
                Group_Nm = group.Group_Nm
            };
        }

        public async Task AddAsync(GroupDTO groupDto)
        {
            var group = new Group
            {
                Group_ID = groupDto.Group_ID,
                Company_ID = groupDto.Company_ID,
                Group_Nm = groupDto.Group_Nm
            };
            _context.Set<Group>().Add(group);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GroupDTO groupDto)
        {
            var group = await _context.Set<Group>().FindAsync(groupDto.Group_ID);
            if (group != null)
            {
                group.Group_Nm = groupDto.Group_Nm;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var group = await _context.Set<Group>().FindAsync(id);
            if (group != null)
            {
                _context.Set<Group>().Remove(group);
                await _context.SaveChangesAsync();
            }
        }
    }
}
