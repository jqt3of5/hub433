namespace Node.Abstractions
{
    public record DeviceCapability (string CapabilityId, string CapabilityTypeId, DeviceCapability.ValueDescriptor [] Values, DeviceCapability.ActionDescriptor [] Actions)
    {
        public record ActionDescriptor(string Name, ValueDescriptor[] Parameters, ValueDescriptor.TypeEnum ReturnType);

        public record ValueDescriptor(string Name, ValueDescriptor.TypeEnum Type)
        {
            public enum TypeEnum
            {
                Int, 
                String,
                Float,
                Void
            }
        }
    }
}