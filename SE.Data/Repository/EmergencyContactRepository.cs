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
    public class EmergencyContactRepository : GenericRepository<EmergencyContact>
    {
        public EmergencyContactRepository() { }
        public EmergencyContactRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<EmergencyContact> GetByElderlyIdAndAccountIdAsync(int elderlyId, int accountId)
        {
            return await _context.EmergencyContacts
                .FirstOrDefaultAsync(ec => ec.ElderlyId == elderlyId && ec.AccountId == accountId);
        }
    }
}
