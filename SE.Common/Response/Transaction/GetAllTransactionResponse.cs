using SE.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Transaction
{
    public class GetAllTransactionResponse
    {
        public int TransactionId { get; set; }
        public string PaymentCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod {  get; set; }
        public string PaymentStatus {  get; set; }
        public string SubscriptionName {  get; set; }
        public string SubscriptionDescription {  get; set; }
        public double Price {  get; set; }
        public UserDTO Account {  get; set; }
    }
}
