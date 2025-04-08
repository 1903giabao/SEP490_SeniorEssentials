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
    public class TimeSlotRepository : GenericRepository<TimeSlot>
    {
        public TimeSlotRepository() { }
        public TimeSlotRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
        public async Task<List<TimeSlot>> GetByProfessorScheduleIdAsync(int professorScheduleId)
        {
            var rs = await _context.TimeSlots
                .Where(ts => ts.ProfessorScheduleId == professorScheduleId )
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
            return rs;
        }        
        
        public async Task<List<TimeSlot>> GetByAndContainProfessorScheduleIdAsync(List<ProfessorSchedule> existingSchedules, string status)
        {
            var rs = await _context.TimeSlots.Include(t => t.ProfessorSchedule)
                .Where(t => existingSchedules.Select(s => s.ProfessorScheduleId).Contains(t.ProfessorScheduleId)
                                              && t.Status == status)
                .ToListAsync();
            return rs;
        }
    }
}
