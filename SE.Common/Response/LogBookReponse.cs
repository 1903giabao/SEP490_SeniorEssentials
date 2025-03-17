using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response
{
    public class LogBookReponse
    {
        public string Tabs { get; set; }
        public int Id { get; set; }
        public string Indicator { get; set; }

        public string DateTime { get; set; }
        public string DateRecorded { get; set; }
        public string TimeRecorded { get; set; }

        public string DataType { get; set; }
        public string Evaluation {  get; set; }
    }
}
