using SE.Data.Models;
using SE.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SE.Data.Repository
{
    public class SubscriptionRepository : GenericRepository<Subscription>
    {
        public SubscriptionRepository() { }
        public SubscriptionRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<Subscription>> GetAllSubscriptions()
        {
            return await _context.Subscriptions.Include(s => s.Bookings).ToListAsync();
        }

        public async Task<List<Subscription>> GetAllUserInSubscriptions(int subId)
        {
            return await _context.Subscriptions.Include(s => s.Bookings)
                                               .ThenInclude(b=>b.Account)
                                               .ToListAsync();
        }
    }
}
