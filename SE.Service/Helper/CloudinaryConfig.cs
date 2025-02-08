using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SE.Service.Helper
{
    public class CloudinaryConfig
    {
        public static Cloudinary GetCloudinary()
        {
            var account = new Account(
                "drtn3fqci",
                "858443377356313",
                "PB_to6cJaRMzhmg9S4yRB9o1WuQ");

            return new Cloudinary(account);
        }
    }
}