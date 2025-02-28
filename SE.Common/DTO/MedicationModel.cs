using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class MedicationModel
    {
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }

        public string Treatment { get; set; }

        public string Shape { get; set; }

        public string Dosage { get; set; }

        public bool? IsBeforeMeal { get; set; }

        public string FrequencyType { get; set; }

        public List<string> FrequencySelect { get; set; }

        public string Note { get; set; }

        public int? Remaining { get; set; }
        public List<ScheduleModel> Schedule { get; set; } 
    }

    public class ScheduleModel
    {
        public string Time { get; set; } 
        public string Status { get; set; } 
    }
}
