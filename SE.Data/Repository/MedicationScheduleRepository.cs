using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SE.Data.Models;
using SE.Data.Base;

namespace SE.Data.Repository
{
    public class MedicationScheduleRepository : GenericRepository<MedicationSchedule>
    {
        public MedicationScheduleRepository() { }
        public MedicationScheduleRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
        public async Task<int> DeleteByMedicationIdAsync(int medicationId)
        {
            var schedulesToDelete = await _context.MedicationSchedules
                .Where(ms => ms.MedicationId == medicationId)
                .ToListAsync();

            if (schedulesToDelete.Any())
            {
                _context.MedicationSchedules.RemoveRange(schedulesToDelete);
                return await _context.SaveChangesAsync(); // Returns the number of rows affected
            }

            return 0; // No schedules found to delete
        }

        public async Task<MedicationSchedule> GetByDateAndMedicationIdAsync(DateTime dateTaken, int medicationId)
        {
            return await _context.MedicationSchedules
                .FirstOrDefaultAsync(ms => ms.MedicationId == medicationId && ms.DateTaken == dateTaken);
        }
    }


}
