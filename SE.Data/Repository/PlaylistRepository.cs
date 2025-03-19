using Microsoft.EntityFrameworkCore;
using SE.Data.Base;
using SE.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class PlaylistRepository : GenericRepository<Playlist>
    {
        public PlaylistRepository() { }
        public PlaylistRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<Playlist>> GetAllPlaylist(string status)
        {
            var result = await _context.Playlists.Include(p => p.Musics).Include(p => p.Lessons).Where(p => p.Status.Equals(status)).ToListAsync();
            return result;
        }
    }
}
