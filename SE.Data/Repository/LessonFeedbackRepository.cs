using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SE.Data.Models;
using SE.Data.Base;

namespace SE.Data.Repository
{
    public class LessonFeedbackRepository : GenericRepository<LessonFeedback>
    {
        public LessonFeedbackRepository() { }
        public LessonFeedbackRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
