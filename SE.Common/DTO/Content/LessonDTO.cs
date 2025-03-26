using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Content
{
    public class LessonDTO
    {
        public int LessonId { get; set; }

        public int? PlaylistId { get; set; }

        public int? AccountId { get; set; }

        public string LessonName { get; set; }

        public string LessonUrl { get; set; }

        public string ImageUrl { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string Status { get; set; }
    }
}
