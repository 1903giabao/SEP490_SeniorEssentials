using SE.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Subscription
{
    public class UserInSubVM
    {
        public int SubscriptionId { get; set; }

        public List<GetUsersInSubscription> UsersInSubscriptions { get; set; }
    }

    public class GetUsersInSubscription
    {
        public UserDTO Buyer { get; set; }
        public UserDTO Elderly { get; set; }
    }
}
