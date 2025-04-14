using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SE.Data.Base;
using SE.Data.Models;

namespace SE.Data.Repository
{
    public class UserLinkRepository : GenericRepository<UserLink>
    {
        public UserLinkRepository() { }
        public UserLinkRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<UserLink>> GetByAccount1Async(int accountId, string status)
        {
            var result = await _context.UserLinks.Include(u => u.AccountId1Navigation).Include(u => u.AccountId2Navigation).Where(ul => ul.AccountId1 == accountId && ul.Status.Equals(status)).ToListAsync();
            return result;
        }                 
        
        public async Task<List<UserLink>> GetByAccount2Async(int accountId, string status)
        {
            var result = await _context.UserLinks.Include(u => u.AccountId1Navigation).Include(u => u.AccountId2Navigation).Where(u => u.AccountId2 == accountId && u.Status.Equals(status)).ToListAsync();
            return result;
        }

        public async Task<UserLink> GetByUserIdsAsync(int userId1, int userId2)
        {
            var result = await _context.UserLinks
                .Include(u => u.AccountId1Navigation)
                .Include(u => u.AccountId2Navigation)
                .FirstOrDefaultAsync(u => (u.AccountId1 == userId1 && u.AccountId2 == userId2) ||
                                           (u.AccountId1 == userId2 && u.AccountId2 == userId1));
            return result;
        }         
        
        public async Task<List<UserLink>> GetByUserIdAsync(int userId, string status)
        {
            var result = await _context.UserLinks
                .Include(u => u.AccountId1Navigation)
                .Include(u => u.AccountId2Navigation)
                .Where(u => (u.AccountId1 == userId || u.AccountId2 == userId) && u.Status.Equals(status) && u.RelationshipType.Equals("Family")).ToListAsync();
            return result;
        }               
    }
}
