using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class MedicationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string Form { get; set; }
        public string Remaining { get; set; } 
        public string TypeFrequency { get; set; }
        public string FrequencyEvery { get; set; }
        public List<string> FrequencySelect { get; set; } 
        public string MealTime { get; set; }
        public List<ScheduleModel> Schedule { get; set; } 
    }

    public class ScheduleModel
    {
        public string Time { get; set; } 
        public string Status { get; set; } 
    }
}
