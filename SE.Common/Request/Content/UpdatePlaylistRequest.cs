using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Content
{
    public class UpdatePlaylistRequest
    {
        public int PlaylistId { get; set; }
        public string PlaylistName { get; set; }
    }
}
