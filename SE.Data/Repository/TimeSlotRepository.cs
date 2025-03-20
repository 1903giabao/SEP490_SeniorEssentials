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
    }
}
