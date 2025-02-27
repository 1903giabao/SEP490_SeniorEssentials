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

        public async Task<UserLink> GetByAccount1Async(int accountId)
        {
            var result = await _context.UserLinks.Include(u => u.AccountId1Navigation).Include(u => u.AccountId2Navigation).FirstOrDefaultAsync(u => u.AccountId1 == accountId);
            return result;
        }        
        
        public async Task<UserLink> GetByAccount2Async(int accountId)
        {
            var result = await _context.UserLinks.Include(u => u.AccountId1Navigation).Include(u => u.AccountId2Navigation).FirstOrDefaultAsync(u => u.AccountId2 == accountId);
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
    }
}
