using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class LessonFeedbackRequest
    {
        public int LessonId { get; set; }
        public int ElderlyId { get; set; }
        public string FeedbackComment { get; set; }
    }
}
