using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Base;
using SE.Data.Models;

namespace SE.Data.Repository
{
    public class BloodGlucoseRepository : GenericRepository<BloodGlucose>
    {
        public BloodGlucoseRepository() { }
        public BloodGlucoseRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
