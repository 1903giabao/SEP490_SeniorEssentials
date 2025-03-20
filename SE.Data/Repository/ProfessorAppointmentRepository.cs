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
    public class ProfessorAppointmentRepository : GenericRepository<ProfessorAppointment>
    {
        public ProfessorAppointmentRepository() { }
        public ProfessorAppointmentRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<ProfessorAppointment>> GetProfessorAppointmentsForDay(int elderlyId, DateOnly date)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            var result = await _context.ProfessorAppointments
                .Include(pa => pa.TimeSlot)
                    .ThenInclude(ts => ts.ProfessorSchedule)
                        .ThenInclude(ps => ps.Professor)
                            .ThenInclude(p => p.Account)
                .Include(pa => pa.Elderly)
                .Where(pa => pa.ElderlyId == elderlyId && pa.AppointmentTime.Date == dateTime)
                .ToListAsync();

            return result;
        }

        public async Task<List<ProfessorAppointment>> GetByElderlyIdAsync(int elderlyId, string type)
        {
            return await _context.ProfessorAppointments
                .Include(pa => pa.TimeSlot)
                .ThenInclude(ts => ts.ProfessorSchedule)
                .Where(pa => pa.ElderlyId == elderlyId && pa.Status == type)
                .ToListAsync();
        }
    }
}
