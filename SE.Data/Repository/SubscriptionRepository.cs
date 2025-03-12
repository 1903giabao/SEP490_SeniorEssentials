using SE.Data.Models;
using SE.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class SubscriptionRepository : GenericRepository<Combo>
    {
        public SubscriptionRepository() { }
        public SubscriptionRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
