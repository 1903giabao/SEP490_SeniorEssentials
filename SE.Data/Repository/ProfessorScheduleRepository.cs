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
    public class ProfessorScheduleRepository : GenericRepository<ProfessorSchedule>
    {
        public ProfessorScheduleRepository() { }
        public ProfessorScheduleRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
        public async Task<List<ProfessorSchedule>> GetByProfessorIdAsync(int professorId)
        {
            var rs = await _context.ProfessorSchedules
                .Where(ps => ps.ProfessorId == professorId && ps.Status == "Active")
                .OrderBy(ps => ps.DayOfWeek)
                .ToListAsync();
            return rs;
        }

        public async Task<List<ProfessorSchedule>> GetProfessorIncludeTimeSlot(int professorId)
        {
            var rs = await _context.ProfessorSchedules
                .Where(ps => ps.ProfessorId == professorId && ps.Status == "Active")
                .Include(ps=>ps.TimeSlots)
                .ToListAsync();
            return rs;
        }
    }
}
