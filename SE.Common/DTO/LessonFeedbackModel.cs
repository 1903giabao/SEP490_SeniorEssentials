using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class LessonFeedbackModel
    {
        public int LessonFeedbackId { get; set; }

        public int LessonId { get; set; }

        public string LessonName { get; set; }

        public int ElderlyId { get; set; }

        public string ELderlyName { get; set; }

        public string FeedbackComment { get; set; }

        public string Status { get; set; }
    }
}
