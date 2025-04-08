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
    public class AccountRepository : GenericRepository<Account>
    {
        public AccountRepository() { }
        public AccountRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            var result = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(e => e.Email == email);
            return result;
        }

        public async Task<Account> GetAccountAsync(int id)
        {
            var result = await _context.Accounts.Include(a => a.Role).Include(a => a.Elderly).Include(a => a.Professor).Include(a => a.ContentProvider).FirstOrDefaultAsync(e => e.AccountId == id);
            return result;
        }

        public async Task<Account> GetByPhoneNumberAsync(string phoneNumber)
        {
            var result = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(e => e.PhoneNumber == phoneNumber);
            return result;
        }
        public async Task<Account> GetElderlyByAccountIDAsync(int accID)
        {
            var result = await _context.Accounts.Include(a => a.Elderly).Include(a => a.Role).FirstOrDefaultAsync(e => e.AccountId == accID);
            return result;
        }

        public async Task<Account> GetProfessorByAccountIDAsync(int accID)
        {
            var result = await _context.Accounts.Include(a => a.Professor).ThenInclude(a => a.Account).Include(a => a.Role).FirstOrDefaultAsync(e => e.AccountId == accID && e.RoleId == 4);
            return result;
        }
    }
}
