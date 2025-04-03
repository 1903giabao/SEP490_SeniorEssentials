using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SE.Common.Request.Report
{
    public class CreateReportRequest
    {
        public int AccountId { get; set; }

        public string ReportTitle { get; set; }

        public string ReportContent { get; set; }

        public IFormFile? Attachment { get; set; }

        public string ReportType { get; set; }

    }
}
