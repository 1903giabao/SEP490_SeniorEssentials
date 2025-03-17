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
    public class BookRepository : GenericRepository<Book>
    {
        public BookRepository() { }
        public BookRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
  
    }
}
