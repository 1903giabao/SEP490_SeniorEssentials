using Microsoft.EntityFrameworkCore;
using SE.Data.Models;
using SE.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class AccountRepository : GenericRepository<Account>
    {
        public AccountRepository() { }
        public AccountRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            var result = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(e => e.Email == email);
            return result;
        }

    }
}
