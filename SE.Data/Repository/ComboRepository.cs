using SE.Data.Models;
using SE.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class ComboRepository : GenericRepository<Combo>
    {
        public ComboRepository() { }
        public ComboRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
