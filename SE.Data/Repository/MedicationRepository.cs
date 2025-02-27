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
    public class MedicationRepository : GenericRepository<Medication>
    {
        public MedicationRepository() { }
        public MedicationRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<Medication>> GetByPrescriptionIdAsync(int prescriptionId)
        {
            return await _context.Medications
                .Include(x => x.Prescription)
                .Where(m => m.PrescriptionId == prescriptionId && m.Prescription.Status == "Active")
                .ToListAsync();
        }

      
    }
}
