using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Rpc;

namespace SE.Common.Response.Professor
{
    public class GetAllAppointmentResponse
    {
        public string ProAvatar {  get; set; }
        public string ProName { get; set; }

        public string ProEmail { get; set; }
        public string ElderlyEmail { get; set; }
        public string ElderlyAvatar { get; set; }
        public string ElderlyFullName { get; set; }

        public string TimeOfAppointment { get; set; }

        public string DateOfAppointment { get; set; }

        public string ReasonOfMeeting { get; set; }
        public string Status {  get; set; }
    }
}
