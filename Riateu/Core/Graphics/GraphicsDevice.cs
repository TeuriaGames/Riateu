using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RefreshCS;
using SDL2;

namespace Riateu.Graphics;

public class GraphicsDevice : IDisposable
{
    public IntPtr Handle { get; internal set; }
    private bool IsDisposed;

    public BackendFlags BackendFlags { get; private set; }
    public bool DebugMode { get; private set; }

    private CommandBuffer deviceCmdBuffer;

    private HashSet<GCHandle> resources = new HashSet<GCHandle>();

    public GraphicsDevice(GraphicsSettings settings, BackendFlags flags) 
    {
        Handle = RefreshCS.Refresh.Refresh_CreateDevice(
            (RefreshCS.Refresh.BackendFlags)flags, 
            settings.DebugMode ? 1 : 0, 
            settings.LowPowerMode ? 1 : 0);

        BackendFlags = (BackendFlags)RefreshCS.Refresh.Refresh_GetBackend(Handle);
        DebugMode = settings.DebugMode;

        SDL.SDL_LogInfo(0, "Graphics Device Created successfully!");
    }

    public void SetSwapchainParameters(Window window, SwapchainComposition swapchainComposition, PresentMode presentMode) 
    {
        if (!window.Claimed) 
        {
            throw new Exception("Cannto change the swapchain parameters when window has not been claimed yet.");
        }
        Refresh.Refresh_SetSwapchainParameters(
            Handle, window.Handle, 
            (Refresh.SwapchainComposition)swapchainComposition, 
            (Refresh.PresentMode)presentMode);
    }

    public bool ClaimWindow(Window window, SwapchainComposition swapchainComposition, PresentMode presentMode) 
    {
        if (window.Claimed) 
        {
            Console.WriteLine("Window has already been claimed");
            return false;
        }
        bool result = RefreshCS.Refresh.Refresh_ClaimWindow(
            Handle, window.Handle, 
            (RefreshCS.Refresh.SwapchainComposition)swapchainComposition, 
            (RefreshCS.Refresh.PresentMode)presentMode) == 1;
        
        if (result) 
        {
            window.Claimed = true;
            window.SwapchainComposition = swapchainComposition;
            window.SwapchainFormat = (TextureFormat)RefreshCS.Refresh.Refresh_GetSwapchainTextureFormat(Handle, window.Handle);
            window.SwapchainTarget = new RenderTarget(this);
        }

        return result;
    }

    public void UnclaimWindow(Window window) 
    {
        if (window.Claimed) 
        {
            RefreshCS.Refresh.Refresh_UnclaimWindow(Handle, window.Handle);
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
        RefreshCS.Refresh.Refresh_Submit(commandBuffer.Handle);
        GraphicsPool<CommandBuffer>.Release(commandBuffer);
    }

    public Fence SubmitAndAcquireFence(CommandBuffer commandBuffer) 
    {
        IntPtr fencePtr = RefreshCS.Refresh.Refresh_SubmitAndAcquireFence(commandBuffer.Handle);
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
        Refresh.Refresh_Wait(Handle);
    }

    public unsafe void WaitForFence(Fence fence) 
    {
        IntPtr fencePtr = fence.Handle;
        Refresh.Refresh_WaitForFences(Handle, 1, &fencePtr, 1);
    }

    public unsafe void WaitForFences(ReadOnlySpan<Fence> fences, bool waitAll) 
    {
        IntPtr* fencePtrs = stackalloc IntPtr[fences.Length];

        for (int i = 0; i < fences.Length; i++) 
        {
            fencePtrs[i] = fences[i].Handle;
        }

        Refresh.Refresh_WaitForFences(Handle, waitAll ? 1 : 0, fencePtrs, (uint)fences.Length);
    }

    public bool QueryFence(Fence fence) 
    {
        int result = Refresh.Refresh_QueryFence(Handle, fence.Handle);

        if (result < 0) 
        {
            throw new Exception("The graphics device has been destroyed!");
        }

        return result != 0;
    }

    public void ReleaseFence(Fence fence) 
    {
        Refresh.Refresh_ReleaseFence(Handle, fence.Handle);
        GraphicsPool<Fence>.Release(fence);
    }

    public void DeviceClaimCommandBuffer(CommandBuffer buffer) 
    {
        deviceCmdBuffer = buffer;
    }

    public RenderPass BeginTarget(RenderTarget target, Color clearColor, bool cycle) 
    {
        return deviceCmdBuffer.BeginRenderPass(new ColorAttachmentInfo(target, clearColor, cycle));
    }

    public RenderPass BeginTarget(RenderTarget target, DepthTarget buffer, Color clearColor, bool cycle) 
    {
        return deviceCmdBuffer.BeginRenderPass(
            new DepthStencilAttachmentInfo(buffer, new DepthStencilValue(buffer.Depth, 0), true, LoadOp.Clear, StoreOp.DontCare, LoadOp.Clear), 
            new ColorAttachmentInfo(target, clearColor, cycle)
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
            RefreshCS.Refresh.Refresh_DestroyDevice(Handle);
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
