using System.Runtime.Serialization;

namespace SmoothAs.Silk.NET.Vulkan;

[Serializable]
public class VulkanException : Exception
{
    public VulkanException() { }
    public VulkanException(string message) : base(message) { }
    public VulkanException(string message, Exception inner) : base(message, inner) { }
    protected VulkanException(SerializationInfo info, StreamingContext context) : base(info, context) 
    { 
    }
}
