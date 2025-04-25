using Microsoft.EntityFrameworkCore;
using SE.Data.Base;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class UserSubscriptionRepository : GenericRepository<UserSubscription>
    {
        public UserSubscriptionRepository() { }
        public UserSubscriptionRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<Professor> GetProfessorByBookingIdAsync(List<int> bookingIds, string status)
        {
            var result = await _context.UserSubscriptions.Include(us => us.Professor).ThenInclude(us => us.Account).Where(us => bookingIds.Contains((int)us.BookingId) && us.Status.Equals(status)).Select(us => us.Professor).FirstOrDefaultAsync();
            return result;
        }

        public async Task<UserSubscription> GetUserSubscriptionByBookingIdAsync(List<int> bookingIds, string status)
        {
            var currentDate = DateTime.UtcNow.AddHours(7);

            var result = await _context.UserSubscriptions
                               .Include(us => us.Professor)
                               .ThenInclude(us => us.Account)
                               .Include(us => us.Booking)
                               .ThenInclude(us => us.Account)                               
                               .Include(us => us.Booking)
                               .ThenInclude(us => us.Subscription)
                                .Where(us => bookingIds.Contains((int)us.BookingId) && us.Status.Equals(status)
                                && us.StartDate <= currentDate &&
                        us.EndDate >= currentDate)
                                .FirstOrDefaultAsync();
            return result;
        }          
        
        public async Task<UserSubscription> GetUserSubscriptionByElderlyAndProfessorAsync(int professorId, int familyMemberId, int elderlyId, string status)
        {
            var currentDate = DateTime.UtcNow.AddHours(7);

            var result = await _context.UserSubscriptions
                               .Include(us => us.Professor)
                               .ThenInclude(us => us.Account)
                               .Include(us => us.Booking)
                               .ThenInclude(us => us.Account)                               
                               .Include(us => us.Booking)
                               .ThenInclude(us => us.Elderly)
                               .ThenInclude(us => us.Account)
                                .Where(us => us.Professor.AccountId == professorId && us.Booking.AccountId == familyMemberId && us.Booking.Elderly.AccountId == elderlyId && us.Status.Equals(status)
                                && us.StartDate <= currentDate &&
                        us.EndDate <= currentDate)
                                .FirstOrDefaultAsync();
            return result;
        }          
        
        public async Task<List<UserSubscription>> GetUserSubscriptionByProfessorAsync(int professorId, string status)
        {
            var currentDate = DateTime.UtcNow.AddHours(7);

            var result = await _context.UserSubscriptions
                               .Include(us => us.Professor)
                               .ThenInclude(us => us.Account)
                               .Include(us => us.Booking)
                               .ThenInclude(us => us.Account)                               
                               .Include(us => us.Booking)
                               .ThenInclude(us => us.Elderly)
                               .ThenInclude(us => us.Account)
                                .Where(us => us.Professor.AccountId == professorId && us.Status.Equals(status)
                                && us.StartDate <= currentDate &&
                        us.EndDate >= currentDate)
                                .ToListAsync();
            return result;
        }        
        
        public async Task<UserSubscription> GetAppointmentUserSubscriptionByBookingIdAsync(List<int> bookingIds, string status)
        {
            var result = await _context.UserSubscriptions.Include(us => us.Professor)
                               .ThenInclude(us => us.Account)
                               .Include(us => us.Booking)
                                .Where(us => bookingIds.Contains((int)us.BookingId) && us.Status.Equals(status)
                                && us.StartDate == us.EndDate)
                                .FirstOrDefaultAsync();
            return result;
        }

        public async Task<UserSubscription> GetProfessorByElderlyId(int elderlyId)
        {
        //    var elderly = await _context.Elderlies.Include(e=>e.Account).FirstOrDefaultAsync(e => e.Account.AccountId == elderlyId);
            var rs = await _context.UserSubscriptions.Include(us => us.Booking)
                                                    .ThenInclude(b => b.Elderly)
                                                    .FirstOrDefaultAsync(us => us.Booking.ElderlyId == elderlyId && us.Status == "Đang khả dụng");
            return rs;
        }
        
        public async Task<List<UserSubscription>> GetAllUserInSubscriptions(int subId)
        {
            return await _context.UserSubscriptions.Include(s => s.Booking).ThenInclude(b => b.Account)
                                                .Include(s => s.Booking).ThenInclude(b => b.Elderly).ThenInclude(b => b.Account)
                                               .Include(s => s.Booking).ThenInclude(b => b.Subscription)
                                               .Include(s => s.Booking).ThenInclude(b => b.Transaction)
                                               .Where(s => s.Booking.SubscriptionId == subId)
                                               .ToListAsync();
        }        
        
        public async Task<List<UserSubscription>> GetAllUserInSubscriptionsInUse(int subId)
        {
            var currentDate = DateTime.UtcNow.AddHours(7);

            return await _context.UserSubscriptions.Include(s => s.Booking).ThenInclude(b => b.Account)
                                                .Include(s => s.Booking).ThenInclude(b => b.Elderly).ThenInclude(b => b.Account)
                                               .Include(s => s.Booking).ThenInclude(b => b.Subscription)
                                               .Where(s => s.Booking.SubscriptionId == subId && s.StartDate <= currentDate &&
                        (s.EndDate == null || s.EndDate >= currentDate))
                                               .ToListAsync();
        }

        public bool CheckIsAvailable(int subId)
        {
            var currentDate = DateTime.UtcNow.AddHours(7);

            var activeUsers = _context.UserSubscriptions.
                                       Include(s => s.Booking)
                                       .ThenInclude(b => b.Subscription)
                                       .ThenInclude(b => b.Account)
                                       .Where(us =>
                        us.Booking.SubscriptionId == subId &&
                        us.StartDate <= currentDate &&
                        (us.EndDate == null || us.EndDate >= currentDate))
                        .Any();
            return activeUsers;

        }

        public async Task<List<Account>> GetListElderlyByProfessorId(int professorId)
        {
            var result = await _context.UserSubscriptions
                .Include(us => us.Professor).ThenInclude(us => us.Account)
                .Include(us => us.Booking).ThenInclude(us => us.Elderly).ThenInclude(us => us.Account).ThenInclude(us => us.Elderly)
                .Where(us => us.ProfessorId == professorId).Select(us => us.Booking.Elderly.Account).ToListAsync();

            var rs = result.DistinctBy(us => us.AccountId).ToList();
            return result;
        }         
        
        public async Task<List<UserSubscription>> GetAllActive(string status)
        {
            var result = await _context.UserSubscriptions
                .Where(us => us.Status.Equals(status)).ToListAsync();
            return result;
        }             
    }
}
