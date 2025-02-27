using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class UpdateMedicationInPrescriptionRequest
    {
        public string Treatment { get; set; }
        public DateOnly? EndDate { get; set; }
        public List<UpdateMedicationModel> Medication { get; set; }
    }

    public class UpdateMedicationModel
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

        public List<string> Schedule { get; set; }

    }

}
