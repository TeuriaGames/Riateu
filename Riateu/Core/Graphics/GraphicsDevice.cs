using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDL3;

namespace Riateu.Graphics;

public class GraphicsDevice : IDisposable
{
    public IntPtr Handle { get; internal set; }
    private bool IsDisposed;

    public static string Backend { get; private set; }
    public static TextureFormat SupportedDepthFormat { get; private set; }
    public static TextureFormat SupportedDepthStencilFormat { get; private set; }
    public bool DebugMode { get; private set; }

    /// <summary>
    /// A globally set shader format. Can be changed if you had a different backend format.
    /// </summary>
    public static ShaderFormat BackendShaderFormat => Backend switch {
        "direct3d12" => ShaderFormat.DXIL,
        _ => ShaderFormat.SPIRV
    };

    private CommandBuffer deviceCmdBuffer;

    private HashSet<GCHandle> resources = new HashSet<GCHandle>();

    public GraphicsDevice(GraphicsSettings settings) 
    {
        Handle = SDL.SDL_CreateGPUDevice((SDL.SDL_GPUShaderFormat)(ShaderFormat.SPIRV | ShaderFormat.DXIL), settings.DebugMode, null);

        if (Handle == IntPtr.Zero) 
        {
            throw new Exception(SDL.SDL_GetError());
        }

        string backend = SDL.SDL_GetGPUDeviceDriver(Handle);

        Backend = backend;
        DebugMode = settings.DebugMode;
        if (SDL.SDL_GPUTextureSupportsFormat(
            Handle, 
            SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D24_UNORM,
            SDL.SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
            SDL.SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_DEPTH_STENCIL_TARGET
        ))
        {
            SupportedDepthFormat = TextureFormat.D24_UNORM;
        }
        else 
        {
            SupportedDepthFormat = TextureFormat.D32_FLOAT;
        }

        if (SDL.SDL_GPUTextureSupportsFormat(
            Handle, 
            SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D24_UNORM_S8_UINT,
            SDL.SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
            SDL.SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_DEPTH_STENCIL_TARGET
        ))
        {
            SupportedDepthStencilFormat = TextureFormat.D24_UNORM_S8_UINT;
        }
        else 
        {
            SupportedDepthStencilFormat = TextureFormat.D32_FLOAT_S8_UINT;
        }
    }

    public void SetSwapchainParameters(Window window, SwapchainComposition swapchainComposition, PresentMode presentMode) 
    {
        if (!window.Claimed) 
        {
            throw new Exception("Cannot change the swapchain parameters when window has not been claimed yet.");
        }

        SDL.SDL_SetGPUSwapchainParameters(
            Handle, window.Handle, 
            (SDL.SDL_GPUSwapchainComposition)swapchainComposition, 
            (SDL.SDL_GPUPresentMode)presentMode);
        window.SwapchainComposition = swapchainComposition;
        window.SwapchainFormat = (TextureFormat)SDL.SDL_GetGPUSwapchainTextureFormat(Handle, window.Handle);
    }

    public bool ClaimWindow(Window window, SwapchainComposition swapchainComposition, PresentMode presentMode) 
    {
        if (window.Claimed) 
        {
            Logger.Error("Window has already been claimed");
            return false;
        }


        bool result = SDL.SDL_ClaimWindowForGPUDevice(Handle, window.Handle);

        if (result) 
        {
            SDL.SDL_SetGPUSwapchainParameters(
                Handle, window.Handle, 
                (SDL.SDL_GPUSwapchainComposition)swapchainComposition, 
                (SDL.SDL_GPUPresentMode)presentMode);
            window.Claimed = true;
            window.SwapchainComposition = swapchainComposition;
            window.SwapchainFormat = (TextureFormat)SDL.SDL_GetGPUSwapchainTextureFormat(Handle, window.Handle);
            window.SwapchainTarget = new RenderTarget(this);
        }
        else 
        {
            Logger.Error(SDL.SDL_GetError());
        }

        return result;
    }

    public void UnclaimWindow(Window window) 
    {
        if (window.Claimed) 
        {
            SDL.SDL_ReleaseWindowFromGPUDevice(Handle, window.Handle);
            window.Claimed = false;
            window.SwapchainTarget.Handle = IntPtr.Zero;
        }
    }

    public CommandBuffer AcquireCommandBuffer() 
    {
        return GraphicsPool<CommandBuffer>.Obtain(this);
    }

    public void Submit(CommandBuffer commandBuffer) 
    {
        SDL.SDL_SubmitGPUCommandBuffer(commandBuffer.Handle);
        GraphicsPool<CommandBuffer>.Release(commandBuffer);
    }

    public Fence SubmitAndAcquireFence(CommandBuffer commandBuffer) 
    {
        IntPtr fencePtr = SDL.SDL_SubmitGPUCommandBufferAndAcquireFence(commandBuffer.Handle);
        GraphicsPool<CommandBuffer>.Release(commandBuffer);
        Fence fence = GraphicsPool<Fence>.Obtain(this);
        fence.Handle = fencePtr;
        return fence;
    }

    public CommandBuffer DeviceCommandBuffer() 
    {
        return deviceCmdBuffer;
    }

    public void Wait() 
    {
        SDL.SDL_WaitForGPUIdle(Handle);
    }

    public void WaitForFence(Fence fence) 
    {
        IntPtr fencePtr = fence.Handle;
        Span<nint> fences = stackalloc nint[1];
        fences[0] = fencePtr;
        SDL.SDL_WaitForGPUFences(Handle, true, fences, 1);
    }

    public void WaitForFences(ReadOnlySpan<Fence> fences, bool waitAll) 
    {
        Span<nint> fencePtrs = stackalloc IntPtr[fences.Length];

        for (int i = 0; i < fences.Length; i++) 
        {
            fencePtrs[i] = fences[i].Handle;
        }

        SDL.SDL_WaitForGPUFences(Handle, true, fencePtrs, (uint)fencePtrs.Length);
    }

    public bool QueryFence(Fence fence) 
    {
        bool result = SDL.SDL_QueryGPUFence(Handle, fence.Handle);

        if (result) 
        {
            throw new Exception("The graphics device has been destroyed!");
        }

        return result;
    }

    public void ReleaseFence(Fence fence) 
    {
        SDL.SDL_ReleaseGPUFence(Handle, fence.Handle);
        GraphicsPool<Fence>.Release(fence);
    }

    public void DeviceClaimCommandBuffer(CommandBuffer buffer) 
    {
        deviceCmdBuffer = buffer;
    }

    public RenderPass BeginTarget(RenderTarget target, Color clearColor, bool cycle) 
    {
        return deviceCmdBuffer.BeginRenderPass(new ColorTargetInfo(target, clearColor, cycle));
    }

    public RenderPass BeginTarget(RenderTarget target, DepthTarget buffer, Color clearColor, bool cycle) 
    {
        return deviceCmdBuffer.BeginRenderPass(
            new DepthStencilTargetInfo(buffer, buffer.LayerCountOrDepth, 0, true, LoadOp.Clear, StoreOp.DontCare, LoadOp.Clear), 
            new ColorTargetInfo(target, clearColor, cycle)
        );
    }

    public void EndTarget(RenderPass renderPass) 
    {
        deviceCmdBuffer.EndRenderPass(renderPass);
    }

    public void AddReference(GCHandle handle) 
    {
        lock (resources) 
        {
            resources.Add(handle);
        }
    }

    public void RemoveReference(GCHandle handle) 
    {
        lock (resources) 
        {
            resources.Remove(handle);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            foreach (var resource in resources) 
            {
                if (resource.Target is IDisposable res) 
                {
                    res.Dispose();
                }
            }
            resources.Clear();
            SDL.SDL_DestroyGPUDevice(Handle);
            IsDisposed = true;
        }
    }

    ~GraphicsDevice()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
