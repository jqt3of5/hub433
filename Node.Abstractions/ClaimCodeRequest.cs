namespace Node.Abstractions
{
    public class ClaimDeviceRequest
    {
        public string ClaimCode { get; set; }
        public string ThingName { get; set; }
    }
    
    public class GenerateClaimCodeRequest
    {
        public string email { get; set; }
        public string guid { get; set; } 
    } 
}