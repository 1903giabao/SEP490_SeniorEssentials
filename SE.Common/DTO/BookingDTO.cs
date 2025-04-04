using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class BookingDTO
    {
        public int BookingId { get; set; }

        public int AccountId { get; set; }

        public int ElderlyId { get; set; }

        public int? SubscriptionId { get; set; }

        public decimal Price { get; set; }

        public DateTime BookingDate { get; set; }

        public string PaymentMethod { get; set; }

        public string Note { get; set; }

        public string Status { get; set; }

        public int? TransactionId { get; set; }

        public int? ProfessorId { get; set; }
    }
}
