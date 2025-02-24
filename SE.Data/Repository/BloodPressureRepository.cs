using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Base;
using SE.Data.Models;

namespace SE.Data.Repository
{
    public class BloodPressureRepository : GenericRepository<BloodPressure>
    {
        public BloodPressureRepository() { }
        public BloodPressureRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
