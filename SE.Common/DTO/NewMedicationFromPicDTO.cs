using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class NewMedicationFromPicDTO
    {
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string TimeFrequency { get; set; }
        public int? DateFrequency { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Shape { get; set; }
        public int Quantity { get; set; }
        public string FrequencyType { get; set; }
        public string Instruction { get; set; }
    }
}
