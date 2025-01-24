using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class MedicationModel
    {
        public int ElderlyId { get; set; }
        public string MedicationName { get; set; }
        public string Treatment { get; set; }
        public string Shape { get; set; }
        public string Dosage { get; set; }
        public bool? IsBeforeMeal { get; set; }
        public string FrequencyType { get; set; }
        public string TimeFrequency { get; set; }
        public int? DateFrequency { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
    }
}
