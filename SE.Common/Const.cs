using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Common
{
    public class Const
    {
        #region
        // Error
        public static int ERROR_EXEPTION = -1;
        #endregion

        #region
        // Warning
        public static int WARNING_NO_DATA = 4;
        public static string WARNING_NO_DATA_MSG = "No data";
        public static int WARNING_INVALID_TOKEN = 4;
        public static string WARNING_INVALID_TOKEN_MSG = "Invalid Token";
        #endregion


        #region
        // Success
        public static int SUCCESS_CREATE = 1;
        public static string SUCCESS_CREATE_MSG = "Create data success";
        public static int SUCCESS_READ = 1;
        public static string SUCCESS_READ_MSG = "Read datasuccess";
        public static int SUCCESS_UDATE = 1;
        public static string SUCCESS_UDATE_MSG = "Update data success";
        public static int SUCCESS_DELETE = 1;
        public static string SUCCESS_DELETE_MSG = "Delete data success";
        public static int SUCCESS_LOGIN = 1;
        public static string SUCCESS_LOGIN_MSG = "Login success";
        public static int SUCCESS_LOGOUT = 1;
        public static string SUCCESS_LOGOUT_MSG = "Logout success";
        public static int SUCCESS_UNACTIVATE = 1;
        public static string SUCCESS_UNACTIVATE_MSG = "Unactivate success";
        public static int SUCCESS_BOOKED = 1;
        public static string SUCCESS_BOOKED_MSG = "Booked success";
        #endregion


        #region
        //Fail
        public static int FAIL_CREATE = 0;
        public static string FAIL_CREATE_MSG = "Create data fail";
        public static int FAIL_READ = 0;
        public static string FAIL_READ_MSG = "Read data fail";
        public static int FAIL_UDATE = 0;
        public static string FAIL_UDATE_MSG = "Update data fail";
        public static int FAIL_DELETE = 0;
        public static string FAIL_DELETE_MSG = "Delete data fail";
        public static int FAIL_LOGIN = 0;
        public static string FAIL_LOGIN_MSG = "Invalid email or password.";
        public static int FAIL_UNACTIVATE = 0;
        public static string FAIL_UNACTIVATE_MSG = "Unactivate fail.";
        public static int FAIL_BOOKING = 1;
        #endregion


    }
}
