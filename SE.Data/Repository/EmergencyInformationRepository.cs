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
    public class EmergencyInformationRepository : GenericRepository<EmergencyInformation>
    {
        public EmergencyInformationRepository() { }
        public EmergencyInformationRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<EmergencyInformation> GetNewestEmergencyInformation(int emergencyId)
        {
            return await _context.EmergencyInformations.Include(ei => ei.EmergencyConfirmation).ThenInclude(ei => ei.ConfirmationAccount)
                .Where(ei => ei.EmergencyConfirmationId == emergencyId)
                .OrderByDescending(ei => ei.DateTime)
                .FirstOrDefaultAsync();
        }        
        
        public async Task<List<EmergencyInformation>> GetListEmergencyInformation(int emergencyId)
        {
            return await _context.EmergencyInformations.Include(ei => ei.EmergencyConfirmation).ThenInclude(ei => ei.ConfirmationAccount)
                .Where(ei => ei.EmergencyConfirmationId == emergencyId)
                .ToListAsync();
        }
    }
}
