using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Booking
{
    public class ZaloPayOrderResponse
    {
        public int return_code { get; set; }
        public string return_message { get; set; }
        public int sub_return_code { get; set; }
        public string sub_return_message { get; set; }
        public string order_url { get; set; }
        public string zp_trans_token { get; set; }
        public string order_token { get; set; }
        public string qr_code { get; set; }
        public string app_trans_id { get; set; }
    }
}
