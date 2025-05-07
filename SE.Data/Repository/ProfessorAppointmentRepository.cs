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

        public async Task<bool> ValidateElderlyProfessorRelationAsync(int appointmentId, int elderlyId, int professorId)
        {
            return await _context.ProfessorAppointments
                .Where(pa => pa.ProfessorAppointmentId == appointmentId && pa.ElderlyId == elderlyId)
                .Join(_context.UserSubscriptions,
                    appointment => appointment.UserSubscriptionId,
                    subscription => subscription.UserSubscriptionId,
                    (appointment, subscription) => subscription)
                .AnyAsync(s => s.ProfessorId == professorId);
        }

        public async Task<ProfessorAppointment> GetAppointmentWithParticipantsAsync(int appointmentId)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.Elderly.Account)
                .Include(a => a.UserSubscription)
                    .ThenInclude(us => us.Professor)
                        .ThenInclude(p => p.Account)
                .FirstOrDefaultAsync(a => a.ProfessorAppointmentId == appointmentId);
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
                           a.AppointmentTime.Date == date.Date && (a.Status == "NotYet" ||a.Status == "Cancelled"))
                .ToListAsync();
        }

        public async Task<ProfessorAppointment> GetUserSubcriptionByAppointmentAsync(int appointmentId)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.UserSubscription)
                    .ThenInclude(us => us.Booking)
                .Include(a => a.UserSubscription)
                    .ThenInclude(us => us.Booking)
                    .ThenInclude(b => b.Subscription)   
                .Include(a => a.UserSubscription)
                    .ThenInclude(us => us.Professor)
                        .ThenInclude(p => p.Account)
                .Include(a=>a.Elderly)
                .Where(a => a.ProfessorAppointmentId == appointmentId)
                .FirstOrDefaultAsync();
        }

        public async Task<ProfessorAppointment> GetByProfessorAppointmentAsync(int professorAppointmentId)
        {
            return await _context.ProfessorAppointments
                .Include(a => a.UserSubscription)
                .ThenInclude(a => a.Booking)
                .ThenInclude(a => a.Account)
                .Include(a => a.UserSubscription)
                .ThenInclude(a => a.Booking)
                .ThenInclude(a => a.Elderly)
                .ThenInclude(a => a.Account)
                .Where(a => a.UserSubscription != null &&
                           a.ProfessorAppointmentId == professorAppointmentId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ProfessorAppointment>> GetAllIncludeSub()
        {
            return await _context.ProfessorAppointments
                                    .Include(p=>p.UserSubscription)
                                    .ThenInclude(p=>p.Professor)
                                    .ThenInclude(p=>p.Account)
                                    .Include(p=>p.Elderly)
                                    .ThenInclude(e=>e.Account)
                                    .ToListAsync();
        }
    }
}
