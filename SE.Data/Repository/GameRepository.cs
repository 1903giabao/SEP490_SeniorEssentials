using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SE.Data.Models;
using StudySpace.Data.Base;

namespace SE.Data.Repository
{
    public class GameRepository : GenericRepository<Game>
    {
        public GameRepository() { }
        public GameRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
