using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Report
{
    public class GetAllReportResponse
    {
        public int ReportId { get; set; }

        public int AccountId { get; set; }

        public string ReportTitle { get; set; }

        public string ReportContent { get; set; }

        public string? AttachmentUrl { get; set; }

        public string ReportType { get; set; }
        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
