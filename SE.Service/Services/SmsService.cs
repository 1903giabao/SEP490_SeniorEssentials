using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SE.Service.Services
{
    public interface ISmsService
    {
        Task<string> SendSmsAsync(string phoneNumber, string otp);

    }

    public class SmsService : ISmsService
    {
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _brandName;

        public SmsService(IConfiguration configuration)
        {
            _apiKey = Environment.GetEnvironmentVariable("SMSApiKey");
            _secretKey = Environment.GetEnvironmentVariable("SMSSecretKey");
            _brandName = Environment.GetEnvironmentVariable("SMSBrandName");
        }


        public async Task<string> SendSmsAsync(string phoneNumber, string otp)
        {
            string content = $"Mã OTP của bạn là : {otp}";
            content = System.Net.WebUtility.UrlEncode(content);

            var url = $"http://api.tinnhanthuonghieu.com/MainService.svc/json/SendMultipleMessage_V4_get?SmsType=2&ApiKey={_apiKey}&SecretKey={_secretKey}&Brandname={_brandName}&Content={content}&Phone={phoneNumber}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result; 
                }
                else
                {
                    throw new Exception("Failed to send SMS.");
                }
            }
        }
        }
}
