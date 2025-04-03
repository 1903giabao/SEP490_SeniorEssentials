using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class GetScheduleOfElderlyByProfessorVM
    {
        public int ElderlyId {  get; set; }
        public string ElderlyName { get; set; }
        public string Avatar {  get; set; }
        public string PhoneNumber { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public List<int> AccountId { get; set; }

        // Add a constructor to initialize the AccountId list
        public GetScheduleOfElderlyByProfessorVM()
        {
            AccountId = new List<int>();
        }
    }
}
