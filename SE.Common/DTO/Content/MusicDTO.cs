using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Content
{
    public class MusicDTO
    {
        public int MusicId { get; set; }

        public int? PlaylistId { get; set; }

        public int? AccountId { get; set; }

        public string MusicName { get; set; }

        public string MusicUrl { get; set; }

        public string Singer { get; set; }

        public string CreatedDate { get; set; }

        public string Status { get; set; }
    }
}
