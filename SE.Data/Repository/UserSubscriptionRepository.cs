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

            var result = await _context.UserSubscriptions.Include(us => us.Professor)
                               .ThenInclude(us => us.Account)
                               .Include(us => us.Booking)
                                .Where(us => bookingIds.Contains((int)us.BookingId) && us.Status.Equals(status)
                                && us.StartDate <= currentDate &&
                        us.EndDate >= currentDate)
                                .FirstOrDefaultAsync();
            return result;
        }

        public async Task<UserSubscription> GetProfessorByElderlyId(int elderlyId)
        {
            var rs = await _context.UserSubscriptions.Include(us => us.Booking).ThenInclude(b => b.Elderly).FirstOrDefaultAsync(us => us.Booking.ElderlyId == elderlyId && us.Status == "Active");
            return rs;
        }

        public async Task<List<UserSubscription>> GetAllUserInSubscriptions(int subId)
        {
            return await _context.UserSubscriptions.Include(s => s.Booking).ThenInclude(b => b.Account)
                                                .Include(s => s.Booking).ThenInclude(b => b.Elderly).ThenInclude(b => b.Account)
                                               .Include(s => s.Booking).ThenInclude(b => b.Subscription)
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
    }
}
