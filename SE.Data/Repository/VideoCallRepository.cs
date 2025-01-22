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
    public class VideoCallRepository : GenericRepository<VideoCall>
    {
        public VideoCallRepository() { }
        public VideoCallRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<VideoCall>> GetAllIncluding()
        {
            var result = await _context.VideoCalls.Include(v => v.Caller).Include(v => v.Receiver).ToListAsync();
            return result;
        }

        public async Task<VideoCall> GetByIdIncluding(int id)
        {
            var result = await _context.VideoCalls.Include(v => v.Caller).Include(v => v.Receiver).FirstOrDefaultAsync(v => v.VideoCallId == id);
            return result;
        }
    }
}
