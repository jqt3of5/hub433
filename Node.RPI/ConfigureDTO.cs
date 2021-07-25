namespace RPINode
{
    public class ThingCreatedResponse
    {
        public string ThingName { get; set; }
        public string CertificatePem { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}