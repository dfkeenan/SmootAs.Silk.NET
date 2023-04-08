using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan.Extensions.EXT;

namespace SmoothAs.Silk.NET.Vulkan;

public unsafe class VulkanDebugUtilMessenger: IDisposable
{
    public const DebugUtilsMessageSeverityFlagsEXT DefaultMessageSeverityFlags
        = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
          DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
          DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;

    public const DebugUtilsMessageTypeFlagsEXT DefaultMessageTypeFlags
        = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
          DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
          DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;

    private ExtDebugUtils debugUtils = default!;
    private DebugUtilsMessengerEXT debugMessenger;
    private Instance instance;

    public VulkanDebugUtilMessenger(DebugUtilsMessageSeverityFlagsEXT messageSeverity = DefaultMessageSeverityFlags, DebugUtilsMessageTypeFlagsEXT messageType = DefaultMessageTypeFlags)
    {
        MessageSeverity = messageSeverity;
        MessageType = messageType;
    }

    public DebugUtilsMessageSeverityFlagsEXT MessageSeverity { get; }
    public DebugUtilsMessageTypeFlagsEXT MessageType { get; }

    internal void PopulateCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
    {
        createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
        createInfo.MessageSeverity = MessageSeverity;
        createInfo.MessageType = MessageType;
        createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;
    }

    internal void Build(Vk api, Instance instance)
    {
        if (!api.TryGetInstanceExtension(instance, out debugUtils))
        {
            throw new VulkanException($"Failed to get extension {ExtDebugUtils.ExtensionName}.");
        }

        DebugUtilsMessengerCreateInfoEXT createInfo = new();
        PopulateCreateInfo(ref createInfo);

        
        ThrowIfInvalid(debugUtils!.CreateDebugUtilsMessenger(instance, in createInfo, null, out debugMessenger), "failed to set up debug messenger!");

        this.instance = instance;
    }

    private uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
    {
        System.Diagnostics.Debug.WriteLine($"validation layer:" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));

        return Vk.False;
    }

    public void Dispose()
    {
        debugUtils!.DestroyDebugUtilsMessenger(instance, debugMessenger, null);
    }
}
