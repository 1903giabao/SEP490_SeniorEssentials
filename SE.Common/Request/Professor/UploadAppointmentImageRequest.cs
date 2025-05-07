using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Professor
{
    public class UploadAppointmentImageRequest
    {
        public int AppointmentId { get; set; }
        public IFormFile Image {  get; set; }
    }
}
