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
    public class PrescriptionRepository : GenericRepository<Prescription>
    {
        public PrescriptionRepository() { }
        public PrescriptionRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
        public async Task<Prescription> GetAllIncludeMedicationInElderly(int elderlyID)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Medications)
                    .ThenInclude(m => m.MedicationSchedules)
                .FirstOrDefaultAsync(p => p.Elderly == elderlyID && p.CreatedAt.Date <= DateTime.UtcNow.AddHours(7) && p.Status.Equals("Active"));
            return prescription;
        }
    }
}
