namespace Node.Abstractions
{
    public record DeviceCapabilities (string [] Values, DeviceCapabilities.ActionDescriptor [] Actions)
    {
        public record ActionDescriptor(string Name, string[] Parameters);
    }
}