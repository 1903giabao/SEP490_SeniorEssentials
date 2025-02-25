using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Base;
using SE.Data.Models;

namespace SE.Data.Repository
{
    public class KidneyFunctionRepository : GenericRepository<KidneyFunction>
    {
        public KidneyFunctionRepository() { }
        public KidneyFunctionRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
