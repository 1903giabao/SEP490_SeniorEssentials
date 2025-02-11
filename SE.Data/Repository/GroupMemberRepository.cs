using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Data.Base;
using Microsoft.EntityFrameworkCore;

namespace SE.Data.Repository
{
    public class GroupMemberRepository : GenericRepository<GroupMember>
    {
        public GroupMemberRepository() { }
        public GroupMemberRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<GroupMember>> GetGroupMembersByAccountIdAsync(int accountId)
        {
            return await _context.GroupMembers
                .Include(x => x.Account)
                .Include(x=>x.Group)
                .Where(gm => gm.AccountId == accountId)
                .ToListAsync();
        }

        public async Task<List<GroupMember>> GetByGroupIdAsync(int groupId)
        {
            return await _context.GroupMembers
                .Include(gm => gm.Group) 
                .Include(x => x.Account)
                .Where(gm => gm.GroupId == groupId)
                .ToListAsync();
        }
        public async Task<GroupMember> GetByGroupIdAndAccountIdAsync(int groupId, int accountId)
        {
            return await _context.GroupMembers
                .Include(x => x.Account)
                .Include(x=>x.Group)
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.AccountId == accountId);
        }
    }
}
