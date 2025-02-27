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
        public async Task<int> DeleteByMedicationIdAsync(int medicationId,DateTime date)
        {

            var schedulesToDelete = await _context.MedicationSchedules
                            .Where(ms => ms.Medication.MedicationId == medicationId &&
                             ms.DateTaken.HasValue &&
                             ms.DateTaken.Value.Date >= date.Date).ToListAsync();

            if (schedulesToDelete.Any())
            {
                _context.MedicationSchedules.RemoveRange(schedulesToDelete);
                return await _context.SaveChangesAsync();
            }

            return 0; 
        }

        public async Task<MedicationSchedule> GetByDateAndMedicationIdAsync(DateTime dateTaken, int medicationId)
        {
            return await _context.MedicationSchedules
                .FirstOrDefaultAsync(ms => ms.MedicationId == medicationId && ms.DateTaken == dateTaken);
        }

        public async Task<List<MedicationSchedule>> GetMedicationSchedulesForDay(int elderlyId, DateOnly date)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            var result = await _context.MedicationSchedules
                .Include(ms => ms.Medication) 
                .Where(ms => ms.Medication.ElderlyId == elderlyId && 
                             ms.DateTaken.HasValue && 
                             ms.DateTaken.Value.Date == dateTime.Date)
                .ToListAsync();

            return result;
        }

    }


}
