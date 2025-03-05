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
    public class UserServiceRepository : GenericRepository<UserService>
    {
        public UserServiceRepository() { }
        public UserServiceRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<Professor> GetProfessorByBookingIdAsync(List<int> bookingIds, string status)
        {
            var result = await _context.UserServices.Include(us => us.Professor).ThenInclude(us => us.Account).Where(us => bookingIds.Contains(us.BookingId) && us.Status.Equals(status)).Select(us => us.Professor).FirstOrDefaultAsync();
            return result;
        }
    }
}
