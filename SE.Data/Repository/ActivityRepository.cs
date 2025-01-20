using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Data.Base;

namespace SE.Data.Repository
{
    public class ActivityRepository : GenericRepository<Activity>
    {
        public ActivityRepository() { }
        public ActivityRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
