using dotenv.net;

namespace SE.API.ConfigModel
{
    public class FirebaseConfigModel
    {
        public string Type {  get; set; }
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


        public FirebaseConfigModel()
        {
            Type = Environment.GetEnvironmentVariable("Chattype");
            ProjectId = Environment.GetEnvironmentVariable("Chatproject_id");
            PrivateKeyId = Environment.GetEnvironmentVariable("Chatprivate_key_id");
            PrivateKey = Environment.GetEnvironmentVariable("Chatprivate_key");
            ClientEmail = Environment.GetEnvironmentVariable("Chatclient_email");
            ClientId = Environment.GetEnvironmentVariable("Chatclient_id");
            AuthUri = Environment.GetEnvironmentVariable("Chatauth_uri");
            TokenUri = Environment.GetEnvironmentVariable("Chattoken_uri");
            AuthProviderx509CertUrl = Environment.GetEnvironmentVariable("Chatauth_provider_x509_cert_url");
            Clientx509CertUrl = Environment.GetEnvironmentVariable("Chatclient_x509_cert_url");
            UniverseDomain = Environment.GetEnvironmentVariable("Chatuniverse_domain");
        }


    }





}
