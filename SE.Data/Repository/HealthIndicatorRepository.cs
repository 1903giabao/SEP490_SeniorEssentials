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
    public class HealthIndicatorRepository : GenericRepository<HealthIndicator>
    {
        public HealthIndicatorRepository() { }
        public HealthIndicatorRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<HealthIndicator>> GetByElderlyIdAsync(int elderlyId)
        {
            return await _context.HealthIndicators
                .Where(e => e.ElderlyId == elderlyId)
                .ToListAsync();
        }
    }
}
