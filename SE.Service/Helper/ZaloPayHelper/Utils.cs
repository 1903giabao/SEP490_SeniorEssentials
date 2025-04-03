using System;

namespace ZaloPay.Helper
{
    public class Utils
    {
        public static long GetTimeStamp(DateTime date) {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        public static long GetTimeStamp(){
            return GetTimeStamp(DateTime.Now);
        }        
        
        public static long GetTimeStampUtc7(DateTime date) {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        public static long GetTimeStampUtc7(){
            return GetTimeStampUtc7(DateTime.UtcNow);
        }

        private static readonly Random rnd = new Random();

        public static string Generate7DigitUniqueId()
        {
            int randomNumber = rnd.Next(1000000, 10000000); 

            long ticks = DateTime.Now.Ticks;
            int ticksMod = (int)(ticks % 1000000); 

            long combined = randomNumber + ticksMod;
            string unique7Digit = (combined % 10000000).ToString("D7");

            return unique7Digit;
        }
    }
}