using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;

namespace SmoothAs.Silk.NET.Vulkan;

public unsafe partial class VulkanInstance : IDisposable
{
    public VulkanInstance(Vk api, Instance native, VulkanDebugUtilMessenger? debugUtilMessenger)
    {
        API = api;
        Native = native;
        DebugUtilMessenger = debugUtilMessenger;
    }

    public Vk API { get; }
    public Instance Native { get; }
    public VulkanDebugUtilMessenger? DebugUtilMessenger { get; }

    public VkHandle ToHandle() => Native.ToHandle();

    public static implicit operator Instance(VulkanInstance instance) => instance.Native;



    public void Dispose()
    {
        DebugUtilMessenger?.Dispose();
        API.DestroyInstance(Native, null);
        API.Dispose();
    }

}
