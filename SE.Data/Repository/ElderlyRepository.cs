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
    public class ElderlyRepository : GenericRepository<Elderly>
    {
        public ElderlyRepository() { }
        public ElderlyRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
        public async Task<Elderly> GetAccountByElderlyId(int elderlyID)
        {
            var rs = await _context.Elderlies.Include(e => e.Account).FirstOrDefaultAsync(e=>e.ElderlyId == elderlyID);
            return rs;
        }
    }
}
