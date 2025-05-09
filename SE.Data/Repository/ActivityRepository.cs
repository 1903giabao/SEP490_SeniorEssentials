using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Data.Base;
using SE.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace SE.Data.Repository
{
    public class ActivityRepository : GenericRepository<Activity>
    {
        public ActivityRepository() { }
        public ActivityRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
        public async Task<List<Activity>> GetActivitiesIncludeOfElderly(int elderlyId)
        {
            var result = await _context.Activities.Include(a => a.ActivitySchedules).Where(a => a.ElderlyId ==elderlyId).ToListAsync();
            return result;
        }        

        public async Task<Activity> GetActivitiesByIdInclude(int activityId)
        {
            var result = await _context.Activities.Include(a => a.ActivitySchedules).Where(a => a.ActivityId == activityId).FirstOrDefaultAsync();
            return result;
        }        

    }
}
