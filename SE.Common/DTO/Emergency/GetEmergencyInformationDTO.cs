using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Emergency
{
    public class GetEmergencyInformationDTO
    {
        public int EmergencyInformationId { get; set; }
        public int EmergencyConfirmationId { get; set; }
        public string FrontCameraImage { get; set; }
        public string RearCameraImage { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
    }
}
