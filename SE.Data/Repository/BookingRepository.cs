using Microsoft.EntityFrameworkCore;
using SE.Data.Models;
using SE.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class BookingRepository : GenericRepository<Booking>
    {
        public BookingRepository() { }
        public BookingRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<Booking> GetByTransactionIdAsync(int transactionId)
        {
            var result = await _context.Bookings
                .Include(b => b.Elderly).ThenInclude(b => b.Account)
                .Include(a => a.Subscription)
                .FirstOrDefaultAsync(e => e.TransactionId == transactionId);
            return result;
        }        
        
        public async Task<List<Booking>> GetByFamilyMemberIdAsync(int familyMemberId, string status)
        {
            var result = await _context.Bookings
                        .Include(a => a.Subscription)
                        .Include(a => a.Elderly).ThenInclude(e => e.Account)
                        .Where(e => e.AccountId == familyMemberId && e.Status.Equals(status)).ToListAsync();
            return result;
        }
    }
}
