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
                .Include(pa=>pa.UserSubscription)
                .ThenInclude(u=>u.Professor)
                .ThenInclude(p=>p.Account)
                .Where(pa => pa.ElderlyId == elderlyId && pa.AppointmentTime.Date == dateTime)
                .ToListAsync();

            return result;
        }

        public async Task<List<ProfessorAppointment>> GetByElderlyIdAsync(int elderlyId, string type)
        {

            if (type == "All")
            {
                return await _context.ProfessorAppointments
                    .Include(p=>p.UserSubscription)
                .Where(pa => pa.ElderlyId == elderlyId)
                .ToListAsync();
            }
            return await _context.ProfessorAppointments
                                    .Include(p => p.UserSubscription)

                .Where(pa => pa.ElderlyId == elderlyId && pa.Status.ToLower() == type.ToLower())
                .ToListAsync();
        }

        public async Task<List<ProfessorAppointment>> GetByProfessorIdAsync(int professorId, string type)
        {
            if (type == "All")
            {
                return await _context.ProfessorAppointments
                    .Include(p => p.UserSubscription)
                    .Where(pa => pa.UserSubscription.ProfessorId == professorId)
                    .ToListAsync();
            }
            return await _context.ProfessorAppointments
                .Include(p => p.UserSubscription)
                .Where(pa => pa.UserSubscription.ProfessorId == professorId &&
                            pa.Status.ToLower() == type.ToLower())
                .ToListAsync();
        }

        public async Task<List<ProfessorAppointment>> GetAppointmentsByProfessorAndDateAsync(int professorId, DateTime date)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.UserSubscription)
                .Where(a => a.UserSubscription != null &&
                           a.UserSubscription.ProfessorId == professorId &&
                           a.AppointmentTime.Date == date.Date
                           && a.Status != "Cancelled")
                .ToListAsync(); 
        }

        public async Task<List<ProfessorAppointment>> GetAppointmentsByProfessorInDateRangeAsync(int professorId, DateTime startDate, DateTime endDate)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.UserSubscription)
                .Where(a => a.UserSubscription != null &&
                           a.UserSubscription.ProfessorId == professorId &&
                           a.AppointmentTime >= startDate &&
                           a.AppointmentTime < endDate)
                .ToListAsync();
        }
        public async Task<List<ProfessorAppointment>> GetByProfessorAndDateRangeAsync(int professorId, DateTime startDate, DateTime endDate)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.UserSubscription)
                .Where(a => a.UserSubscription != null &&
                           a.UserSubscription.ProfessorId == professorId &&
                           a.AppointmentTime >= startDate &&
                           a.AppointmentTime <= endDate)
                .ToListAsync();
        }

        public async Task<List<ProfessorAppointment>> GetByProfessorAndDateAsync(int professorId, DateTime date)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.UserSubscription)
                .Where(a => a.UserSubscription != null &&
                           a.UserSubscription.ProfessorId == professorId &&
                           a.AppointmentTime.Date == date.Date)
                .ToListAsync();
        }

        public async Task<ProfessorAppointment> GetUserSubcriptionByAppointmentAsync(int appointmentId)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.UserSubscription)
                .ThenInclude(us => us.Booking)
                .ThenInclude(b => b.Subscription)
                .Where(a => a.ProfessorAppointmentId == appointmentId)
                .FirstOrDefaultAsync();
        }
    }
}
