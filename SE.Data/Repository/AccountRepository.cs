using Microsoft.EntityFrameworkCore;
using SE.Data.Models;
using StudySpace.Data.Base;
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

        
    }
}
