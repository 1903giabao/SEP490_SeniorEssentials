using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Enums
{
    public class SD
    {
        private static SD instance;
        private SD()
        {
        }
        public static SD getInstance()
        {
            if (instance == null) instance = new SD();
            return instance;
        }

        public class GeneralStatus
        {
            public static string ACTIVE = "Active";
            public static string INACTIVE = "Inactive";
        }        
        
        public class UserLinkStatus
        {
            public static string PENDING = "Pending";
            public static string CANCELLED = "Cancelled";
            public static string ACCEPTED = "Accepted";
            public static string DELETED = "Deleted";
        }
    }
}
