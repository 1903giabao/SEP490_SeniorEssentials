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

        public async Task<Group> GetGroupOfElderly(int elderlyId)
        {
            return await _context.GroupMembers
                        .Include(gm => gm.Account)
                        .Include(gm => gm.Group)
                        .Where(gm => gm.AccountId == elderlyId)
                        .Select(gm => gm.Group)
                        .FirstOrDefaultAsync();
        }          
        
        public async Task<List<Group>> GetGroupOfFamilyMember(int elderlyId)
        {
            return await _context.GroupMembers
                        .Include(gm => gm.Account)
                        .Include(gm => gm.Group)
                        .Where(gm => gm.AccountId == elderlyId)
                        .Select(gm => gm.Group)
                        .ToListAsync();
        }           
        
        public async Task<List<Account>> GetFamilyMemberInGroup(int groupId)
        {
            return await _context.GroupMembers
                        .Include(gm => gm.Account)
                        .Include(gm => gm.Group)
                        .Where(gm => gm.GroupId == groupId && gm.Account.RoleId == 3)
                        .Select(gm => gm.Account)
                        .ToListAsync();
        }         
        
        public async Task<List<int>> GetFamilyMemberInGroupByGroupIdAsync(int groupId, string status)
        {
            return await _context.GroupMembers.Include(gm => gm.Account)
                    .Where(gm => gm.GroupId == groupId && gm.Status.Equals(status) && gm.Account.RoleId == 3)
                    .Select(gm => gm.AccountId)
                    .Distinct()
                    .ToListAsync();
        }         
        
        public async Task<List<Account>> GetAccountFamilyMemberInGroupByGroupIdAsync(int groupId, string status)
        {
            return await _context.GroupMembers.Include(gm => gm.Account)
                    .Where(gm => gm.GroupId == groupId && gm.Status.Equals(status) && gm.Account.RoleId == 3)
                    .Select(gm => gm.Account)
                    .Distinct()
                    .ToListAsync();
        }        
        
        public async Task<List<Elderly>> GetElderlyInGroupByGroupIdAsync(int groupId, string status)
        {
            return await _context.GroupMembers.Include(gm => gm.Account).ThenInclude(gm => gm.Elderly).ThenInclude(gm => gm.Account)
                    .Where(gm => gm.GroupId == groupId && gm.Status.Equals(status) && gm.Account.RoleId == 2)
                    .Select(gm => gm.Account.Elderly)
                    .Distinct()
                    .ToListAsync();
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
