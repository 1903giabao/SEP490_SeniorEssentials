using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Content
{
    public class PlaylistDTO
    {
        public int PlaylistId { get; set; }
        public string PlaylistName { get; set; }
        public string ImageUrl { get; set; }
        public int NumberOfContent { get; set; }
    }
}
