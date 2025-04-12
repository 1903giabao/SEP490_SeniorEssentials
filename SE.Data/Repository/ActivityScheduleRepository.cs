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
    public class ActivityScheduleRepository : GenericRepository<ActivitySchedule>
    {
        public ActivityScheduleRepository() { }
        public ActivityScheduleRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<ActivitySchedule>> GetAllAsync()
        {
            var rs = await _context.ActivitySchedules.Include(a=>a.Activity).ToListAsync();
            return rs;
        }

        public async Task<int> DeleteActivitySchedulesByActivityIdAsync(int activityId)
        {
            var schedulesToDelete = await _context.ActivitySchedules
                                                 .Where(s => s.ActivityId == activityId)
                                                 .ToListAsync();

            if (schedulesToDelete.Any())
            {
                _context.ActivitySchedules.RemoveRange(schedulesToDelete);

                return await _context.SaveChangesAsync();
            }

            return 0;
        }
        public async Task<List<ActivitySchedule>> GetByElderlyIdAsync(int elderlyId)
        {
            return await _context.ActivitySchedules
                .Include(s => s.Activity)
                .Where(s => s.Activity.ElderlyId == elderlyId &&
                           s.Status == "Active")
                .ToListAsync();
        }
    }
}
