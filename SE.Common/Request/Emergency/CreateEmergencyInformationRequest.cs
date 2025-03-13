using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Emergency
{
    public class CreateEmergencyInformationRequest
    {
        public int EmergencyConfirmationId { get; set; }
        public IFormFile FrontCameraImage { get; set; }
        public IFormFile RearCameraImage { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public bool CallProfessor { get; set; }
        public bool IsSendMessage { get; set; }
    }
}
