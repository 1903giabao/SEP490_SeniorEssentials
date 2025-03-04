namespace SE.API.ConfigModel
{
    public class GGCloudVisionApiConfigModel
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


        public GGCloudVisionApiConfigModel()
        {
            Type = Environment.GetEnvironmentVariable("Scantype");
            ProjectId = Environment.GetEnvironmentVariable("Scanproject_id");
            PrivateKeyId = Environment.GetEnvironmentVariable("Scanprivate_key_id");
            PrivateKey = Environment.GetEnvironmentVariable("Scanprivate_key");
            ClientEmail = Environment.GetEnvironmentVariable("Scanclient_email");
            ClientId = Environment.GetEnvironmentVariable("Scanclient_id");
            AuthUri = Environment.GetEnvironmentVariable("Scanauth_uri");
            TokenUri = Environment.GetEnvironmentVariable("Scantoken_uri");
            AuthProviderx509CertUrl = Environment.GetEnvironmentVariable("Scanauth_provider_x509_cert_url");
            Clientx509CertUrl = Environment.GetEnvironmentVariable("Scanclient_x509_cert_url");
            UniverseDomain = Environment.GetEnvironmentVariable("Scanuniverse_domain");
        }
    }
}
