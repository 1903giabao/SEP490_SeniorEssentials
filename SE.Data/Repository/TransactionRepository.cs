using Microsoft.EntityFrameworkCore;
using SE.Data.Base;
using SE.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class TransactionRepository : GenericRepository<Transaction>
    {
        public TransactionRepository() { }
        public TransactionRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetAllTransaction()
        {
            return await _context.Transactions
                    .Include(gm => gm.Booking).ThenInclude(t => t.Account)
                    .Include(gm => gm.Booking).ThenInclude(t => t.Subscription)
                    .Include(t => t.Account)
                    .ToListAsync();
        }
    }
}
