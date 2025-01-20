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
    public class ProfessorRatingRepository : GenericRepository<ProfessorRating>
    {
        public ProfessorRatingRepository() { }
        public ProfessorRatingRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }

}
