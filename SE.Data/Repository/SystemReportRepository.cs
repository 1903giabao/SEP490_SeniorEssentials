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
    public class SystemReportRepository: GenericRepository<SystemReport>
    {
        public SystemReportRepository() { }
        public SystemReportRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<SystemReport>> GetAllIncludeAccount()
        {
            return await _context.SystemReports.Include(s=>s.Account).ToListAsync();
        }
    }
}
