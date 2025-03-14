using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Base;
using SE.Data.Models;

namespace SE.Data.Repository
{
    public class HealthIndicatorBaseRepository : GenericRepository<HealthIndicatorBase>
    {
        public HealthIndicatorBaseRepository() { }
        public HealthIndicatorBaseRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
