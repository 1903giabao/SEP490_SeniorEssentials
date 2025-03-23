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

        public async Task<UserSubscription> GetProfessorByElderlyId(int elderlyId)
        {
            var rs = await _context.UserSubscriptions.Include(us => us.Booking).ThenInclude(b => b.Elderly).FirstOrDefaultAsync(us=>us.Booking.ElderlyId == elderlyId && us.Status == "Active");
            return rs;
        }
    }
}
