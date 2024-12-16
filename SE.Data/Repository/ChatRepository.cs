using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using StudySpace.Data.Base;

namespace SE.Data.Repository
{
    public class ChatRepository : GenericRepository<Chat>
    {
        public ChatRepository() { }
        public ChatRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
