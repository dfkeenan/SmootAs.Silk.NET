using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan.Extensions.EXT;

namespace SmoothAs.Silk.NET.Vulkan;

public unsafe partial class VulkanInstance
{

    public static VulkanInstanceBuilder CreateBuilder(IVkSurfaceSource surfaceSource) => new VulkanInstanceBuilder(Vk.GetApi(), surfaceSource);
    public static VulkanInstanceBuilder CreateBuilder(IVkSurfaceSource surfaceSource,Vk vk) => new VulkanInstanceBuilder(vk, surfaceSource);
}

public unsafe sealed class VulkanInstanceBuilder
{
	private readonly Vk vk;
    private readonly IVkSurfaceSource surfaceSource;

    private readonly List<string> layers = new();
    private bool enableValidationLayers = false;

    //ApplicationInfo
    private string applicationName = "Vulkan Application";
    private Version32 applicationVersion = new Version32(1, 0, 0);
    private string engineName = "No Engine";
    private Version32 engineVersion = new Version32(1, 0, 0);
    private Version32 apiVersion = Vk.Version12;

    private VulkanDebugUtilMessenger? debugUtilMessenger;

    //private readonly List<string> extensions = new();

    internal VulkanInstanceBuilder(Vk vk, IVkSurfaceSource surfaceSource)
    {
        this.vk = vk ?? throw new ArgumentNullException(nameof(vk));
        this.surfaceSource = surfaceSource ?? throw new ArgumentNullException(nameof(surfaceSource));
    }

    public VulkanInstanceBuilder WithDefaults()
        => this.EnableValidationLayers();

    public VulkanInstanceBuilder EnableValidationLayers(IEnumerable<string> validationLayers)
    {
        if (validationLayers is null)
        {
            throw new ArgumentNullException(nameof(validationLayers));
        }

        if(enableValidationLayers) return this;

        enableValidationLayers = true;
        this.layers.AddRange(validationLayers);

        debugUtilMessenger ??= new VulkanDebugUtilMessenger();

        return this;
    }

    public VulkanInstanceBuilder EnableValidationLayers()
        => EnableValidationLayers(new[]
                                    {
                                        "VK_LAYER_KHRONOS_validation"
                                    });

    public VulkanInstance Build()
    {
        if (!CheckLayerSupport())
        {
            throw new VulkanException("Layers requested, but not available.");
        }

        var appInfo = new ApplicationInfo()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi(applicationName),
            ApplicationVersion = applicationVersion,
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi(engineName),
            EngineVersion = engineVersion,
            ApiVersion = apiVersion,
        };

        var extensions = GetRequiredExtensions();

        var createInfo = new InstanceCreateInfo()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,

            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions),

            EnabledLayerCount = 0,
            PNext = null,           
        };

        if (enableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)layers.Count;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(layers);

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
            debugUtilMessenger!.PopulateCreateInfo(ref debugCreateInfo);
            createInfo.PNext = &debugCreateInfo;
        }

        ThrowIfInvalid(vk.CreateInstance(createInfo, null, out var instance), "Failed to create Instance.");

        Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

        if (enableValidationLayers)
        {
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            debugUtilMessenger!.Build(vk, instance);
        }

        var result = new VulkanInstance(vk, instance, debugUtilMessenger);

        return result;
    }

    private string[] GetRequiredExtensions()
    {
        var surfaceExtensions = surfaceSource.VkSurface!.GetRequiredExtensions(out var surfaceExtensionCount);
        var extensions = SilkMarshal.PtrToStringArray((nint)surfaceExtensions, (int)surfaceExtensionCount);

        if (enableValidationLayers)
        {
            return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
        }

        return extensions;
    }

    private bool CheckLayerSupport()
    {
        if (!layers.Any()) return true;

        uint layerCount = 0;
        vk.EnumerateInstanceLayerProperties(ref layerCount, null);
        var availableLayers = new LayerProperties[layerCount];
        fixed (LayerProperties* pAvailableLayers = availableLayers)
        {
            vk.EnumerateInstanceLayerProperties(ref layerCount, pAvailableLayers);
        }

        var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

        return layers!.All(availableLayerNames.Contains);
    }
}
