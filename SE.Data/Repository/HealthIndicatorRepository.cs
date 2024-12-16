using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using StudySpace.Data.Base;

namespace SE.Data.Repository
{
    public class HealthIndicatorRepository : GenericRepository<HealthIndicator>
    {
        public HealthIndicatorRepository() { }
        public HealthIndicatorRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
