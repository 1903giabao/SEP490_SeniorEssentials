using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class LessonHistoryModel
    {
        public int LessonHistoryId { get; set; }

        public int LessonId { get; set; }

        public string LessonName { get; set; }

        public int ElderlyId { get; set; }

        public string ELderlyName { get; set; }

        public DateTime? StartTime { get; set; }

        public bool IsCompleted { get; set; }

        public string Status { get; set; }
    }
}
