using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class ProfessorScheduleModel
    {
        public int ProfessorScheduleId { get; set; }

        public int ProfessorId { get; set; }

        public DateOnly DayOfWeek { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public string Status { get; set; }
    }
}
