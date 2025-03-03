using Google.Apis.Storage.v1.Data;

namespace SE.API.ConfigModel
{
    public class FCMConfigModel
    {
        public string Type { get; set; }
        public string ProjectId { get; set; }
        public string PrivateKeyId { get; set; }
        public string PrivateKey { get; set; }
        public string ClientEmail { get; set; }
        public string ClientId { get; set; }
        public string AuthUri { get; set; }
        public string TokenUri { get; set; }
        public string AuthProviderx509CertUrl { get; set; }
        public string Clientx509CertUrl { get; set; }
        public string UniverseDomain { get; set; }


        public FCMConfigModel()
        {
            Type = Environment.GetEnvironmentVariable("CloudMessagetype");
            ProjectId = Environment.GetEnvironmentVariable("CloudMessageproject_id");
            PrivateKeyId = Environment.GetEnvironmentVariable("CloudMessageprivate_key_id");
            PrivateKey = Environment.GetEnvironmentVariable("CloudMessageprivate_key");
            ClientEmail = Environment.GetEnvironmentVariable("CloudMessageclient_email");
            ClientId = Environment.GetEnvironmentVariable("CloudMessageclient_id");
            AuthUri = Environment.GetEnvironmentVariable("CloudMessageauth_uri");
            TokenUri = Environment.GetEnvironmentVariable("CloudMessagetoken_uri");
            AuthProviderx509CertUrl = Environment.GetEnvironmentVariable("CloudMessageauth_provider_x509_cert_url");
            Clientx509CertUrl = Environment.GetEnvironmentVariable("CloudMessageclient_x509_cert_url");
            UniverseDomain = Environment.GetEnvironmentVariable("CloudMessageuniverse_domain");
        }
    }
}
