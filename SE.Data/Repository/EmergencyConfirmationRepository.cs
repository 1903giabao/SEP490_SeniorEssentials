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
    public class EmergencyConfirmationRepository: GenericRepository<EmergencyConfirmation>
    {
        public EmergencyConfirmationRepository() { }
        public EmergencyConfirmationRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<EmergencyConfirmation> GetEmergencyConfirmationByIdAsync(int id)
        {
            return await _context.EmergencyConfirmations.Include(ec => ec.Elderly).ThenInclude(e => e.Account).FirstOrDefaultAsync(ec => ec.EmergencyConfirmationId == id);
        }           
        
        public async Task<List<EmergencyConfirmation>> GetListEmergencyConfirmationByElderlyIdAsync(int id)
        {
            return await _context.EmergencyConfirmations.Include(ec => ec.Elderly).ThenInclude(e => e.Account).Where(ec => ec.ElderlyId == id).ToListAsync();
        }              
    }
}
