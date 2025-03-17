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
    public class MusicRepository : GenericRepository<Music>
    {
        public MusicRepository() { }
        public MusicRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<Music>> GetMusicsByPlaylist(int playlistId)
        {
            var result = await _context.Musics.Include(l => l.Playlist).Where(l => l.PlaylistId == playlistId && l.Status.Equals("Active")).ToListAsync();
            return result;
        }
    }
}
