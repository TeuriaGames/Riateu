using System;
using System.Runtime.CompilerServices;
using SDL3;

namespace Riateu.Graphics;

public class CommandBuffer : IGraphicsPool
{   
    public IntPtr Handle { get; internal set; }
    public GraphicsDevice Device { get; internal set; }

#if DEBUG
    private bool renderPassActive;
    private bool computePassActive;
    private bool copyPassActive;
    private bool acquiredSwapChainTarget;

    public bool Submitted;
#endif

    public RenderTarget AcquireSwapchainTarget(Window window) 
    {
#if DEBUG
        AssertNotSubmitted();
        if (!window.Claimed) 
        {
            throw new InvalidOperationException("Cannot acquire swapchain target, the window has not been claimed yet.");
        }
        if (acquiredSwapChainTarget) 
        {
            throw new InvalidOperationException("The swapchain target has already been acquired in this command buffer.");
        }
        acquiredSwapChainTarget = true;
#endif
        var success = SDL.SDL_AcquireGPUSwapchainTexture(Handle, window.Handle, out nint texPtr, out uint w, out uint h);

        if (!success || texPtr == IntPtr.Zero) 
        {
            return null;
        }

        window.SwapchainTarget.Handle = texPtr;
        window.SwapchainTarget.Width = w;
        window.SwapchainTarget.Height = h;
        window.SwapchainTarget.Format = window.SwapchainFormat;

        return window.SwapchainTarget;
    }

    public void Blit(in BlitRegion source, in BlitRegion destination, Filter filter, bool cycle) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
#endif
        SDL.SDL_GPUBlitInfo info = new SDL.SDL_GPUBlitInfo 
        {
            source = source.ToSDLGpu(),
            destination = destination.ToSDLGpu(),
            filter = (SDL.SDL_GPUFilter)filter,
            cycle = cycle
        };
        SDL.SDL_BlitGPUTexture(Handle, info);
    }

    public unsafe RenderPass BeginRenderPass(in ColorTargetInfo info) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[1];
        infos[0] = info.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 1, Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>());
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in ColorTargetInfo info, in ColorTargetInfo info2) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);

        AssertTextureIsNotNull(info2);
        AssertColorIsRenderTarget(info2);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[2];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 2, Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>());
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in ColorTargetInfo info, in ColorTargetInfo info2, in ColorTargetInfo info3) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);

        AssertTextureIsNotNull(info2);
        AssertColorIsRenderTarget(info2);

        AssertTextureIsNotNull(info3);
        AssertColorIsRenderTarget(info3);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[3];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 3, Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>());
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in ColorTargetInfo info, in ColorTargetInfo info2, in ColorTargetInfo info3, in ColorTargetInfo info4) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);

        AssertTextureIsNotNull(info2);
        AssertColorIsRenderTarget(info2);

        AssertTextureIsNotNull(info3);
        AssertColorIsRenderTarget(info3);

        AssertTextureIsNotNull(info4);
        AssertColorIsRenderTarget(info4);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[4];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();
        infos[3] = info3.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 4, Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>());
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(Span<ColorTargetInfo> infoSpan) 
    {
        int length = infoSpan.Length;
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        for (int i = 0; i < length; i++) 
        {
            ColorTargetInfo info = infoSpan[i];
            AssertTextureIsNotNull(info);
            AssertColorIsRenderTarget(info);
        }

        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[length];
        for (int i = 0; i < length; i++) 
        {
            infos[i] = infoSpan[i].ToSDLGpu();
        }

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, (uint)length, Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>());
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilTargetInfo depthStencilAttachment, in Span<ColorTargetInfo> infoSpan) 
    {
        int length = infoSpan.Length;
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();


        for (int i = 0; i < length; i++) 
        {
            ColorTargetInfo info = infoSpan[i];
            AssertTextureIsNotNull(info);
            AssertColorIsRenderTarget(info);
        }
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[length];
        for (int i = 0; i < length; i++) 
        {
            infos[i] = infoSpan[i].ToSDLGpu();
        }

        SDL.SDL_GPUDepthStencilTargetInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, (uint)length, dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilTargetInfo depthStencilAttachment, in ColorTargetInfo info) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[1];
        infos[0] = info.ToSDLGpu();

        SDL.SDL_GPUDepthStencilTargetInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 1, dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilTargetInfo depthStencilAttachment, in ColorTargetInfo info, in ColorTargetInfo info2) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);

        AssertTextureIsNotNull(info2);
        AssertColorIsRenderTarget(info2);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[2];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();

        SDL.SDL_GPUDepthStencilTargetInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 2, dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilTargetInfo depthStencilAttachment, in ColorTargetInfo info, in ColorTargetInfo info2, in ColorTargetInfo info3) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);

        AssertTextureIsNotNull(info2);
        AssertColorIsRenderTarget(info2);

        AssertTextureIsNotNull(info3);
        AssertColorIsRenderTarget(info3);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[3];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();
        SDL.SDL_GPUDepthStencilTargetInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 3, dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilTargetInfo depthStencilAttachment, in ColorTargetInfo info, in ColorTargetInfo info2, in ColorTargetInfo info3, in ColorTargetInfo info4) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);

        AssertTextureIsNotNull(info2);
        AssertColorIsRenderTarget(info2);

        AssertTextureIsNotNull(info3);
        AssertColorIsRenderTarget(info3);

        AssertTextureIsNotNull(info4);
        AssertColorIsRenderTarget(info4);
        renderPassActive = true;
#endif
        Span<SDL.SDL_GPUColorTargetInfo> infos = stackalloc SDL.SDL_GPUColorTargetInfo[4];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();
        infos[3] = info3.ToSDLGpu();

        SDL.SDL_GPUDepthStencilTargetInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, infos, 3, dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public void EndRenderPass(RenderPass renderPass) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertIsPassActive("Render", renderPassActive);
        renderPassActive = false;
#endif
        SDL.SDL_EndGPURenderPass(renderPass.Handle);
        PassPool<RenderPass>.Release(renderPass);
    }

    public CopyPass BeginCopyPass() 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
        copyPassActive = true;
#endif
        IntPtr copyPassPtr = SDL.SDL_BeginGPUCopyPass(Handle);

        CopyPass copyPass = PassPool<CopyPass>.Obtain(copyPassPtr);
        return copyPass;
    }

    public void EndCopyPass(CopyPass copyPass) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertIsPassActive("Copy", copyPassActive);
        copyPassActive = false;
#endif
        SDL.SDL_EndGPUCopyPass(copyPass.Handle);
        PassPool<CopyPass>.Release(copyPass);
    }

    public unsafe ComputePass BeginComputePass(in StorageBufferReadWriteBinding readWriteBinding) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
        computePassActive = true;
#endif
        SDL.SDL_GPUStorageBufferReadWriteBinding storage = readWriteBinding.ToSDLGpu();
        Span<SDL.SDL_GPUStorageBufferReadWriteBinding> storages = stackalloc SDL.SDL_GPUStorageBufferReadWriteBinding[1];
        storages[0] = storage;

        IntPtr computePassPtr = SDL.SDL_BeginGPUComputePass(Handle, new Span<SDL.SDL_GPUStorageTextureReadWriteBinding>(), 0, storages, 1);

        ComputePass computePass = PassPool<ComputePass>.Obtain(computePassPtr);
        return computePass;
    }

    public unsafe ComputePass BeginComputePass(in StorageTextureReadWriteBinding readWriteBinding) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
        computePassActive = true;
#endif
        SDL.SDL_GPUStorageTextureReadWriteBinding storage = readWriteBinding.ToSDLGpu();
        Span<SDL.SDL_GPUStorageTextureReadWriteBinding> storages = stackalloc SDL.SDL_GPUStorageTextureReadWriteBinding[1];
        storages[0] = storage;
        IntPtr computePassPtr = SDL.SDL_BeginGPUComputePass(Handle, storages, 1, new Span<SDL.SDL_GPUStorageBufferReadWriteBinding>(), 0);

        ComputePass computePass = PassPool<ComputePass>.Obtain(computePassPtr);
        return computePass;
    }

    public unsafe ComputePass BeginComputePass(in StorageBufferReadWriteBinding buffer, in StorageTextureReadWriteBinding texture) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
        computePassActive = true;
#endif
        SDL.SDL_GPUStorageBufferReadWriteBinding bufStorage = buffer.ToSDLGpu();
        Span<SDL.SDL_GPUStorageBufferReadWriteBinding> bufStorages = stackalloc SDL.SDL_GPUStorageBufferReadWriteBinding[1];
        bufStorages[0] = bufStorage;

        SDL.SDL_GPUStorageTextureReadWriteBinding texStorage = texture.ToSDLGpu();
        Span<SDL.SDL_GPUStorageTextureReadWriteBinding> texStorages = stackalloc SDL.SDL_GPUStorageTextureReadWriteBinding[1];
        texStorages[0] = texStorage;
        IntPtr computePassPtr = SDL.SDL_BeginGPUComputePass(Handle, texStorages, 1, bufStorages, 1);

        ComputePass computePass = PassPool<ComputePass>.Obtain(computePassPtr);
        return computePass;
    }

    public void EndComputePass(ComputePass computePass) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertIsPassActive("Compute", computePassActive);
        computePassActive = false;
#endif
        SDL.SDL_EndGPUComputePass(computePass.Handle);
        PassPool<ComputePass>.Release(computePass);
    }

	/// <summary>
	/// Pushes data to a vertex uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a render or compute pass.
	/// </summary>
	public unsafe void PushVertexUniformData(void* uniformsPtr, uint size, uint slot = 0) 
    {
        SDL.SDL_PushGPUVertexUniformData(Handle, slot, (nint)uniformsPtr, size);
	}

	/// <summary>
	/// Pushes data to a vertex uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a render or compute pass.
	/// </summary>
	public unsafe void PushVertexUniformData<T>(in T uniforms, uint slot = 0) 
    where T : unmanaged
	{
		fixed (T* uniformsPtr = &uniforms)
		{
			PushVertexUniformData(uniformsPtr, (uint) sizeof(T), slot);
		}
	}

	/// <summary>
	/// Pushes data to a fragment uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
	public unsafe void PushFragmentUniformData(void* uniformsPtr, uint size, uint slot = 0) 
    {
        SDL.SDL_PushGPUFragmentUniformData(Handle, slot, (nint)uniformsPtr, size);
	}

	/// <summary>
	/// Pushes data to a fragment uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
	public unsafe void PushFragmentUniformData<T>(in T uniforms, uint slot = 0) 
    where T : unmanaged 
    {
		fixed (T* uniformsPtr = &uniforms)
		{
			PushFragmentUniformData(uniformsPtr, (uint)sizeof(T), slot);
		}
	}

	/// <summary>
	/// Pushes data to a compute uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
	public unsafe void PushComputeUniformData(void* uniformsPtr, uint size, uint slot = 0) 
    {
        SDL.SDL_PushGPUComputeUniformData(Handle, slot, (nint)uniformsPtr, size);
	}

	/// <summary>
	/// Pushes data to a compute uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
	public unsafe void PushComputeUniformData<T>(
		in T uniforms,
		uint slot = 0
	) where T : unmanaged
	{
		fixed (T* uniformsPtr = &uniforms)
		{
			PushComputeUniformData(uniformsPtr, (uint) sizeof(T), slot);
		}
	}

    public void Obtain(GraphicsDevice device)
    {
        Device = device;
        Handle = SDL.SDL_AcquireGPUCommandBuffer(device.Handle);
#if DEBUG
        Submitted = false;
#endif
    }

    public void Reset()
    {
        Handle = IntPtr.Zero;
#if DEBUG   
        renderPassActive = false;
        computePassActive = false;
        copyPassActive = false;
        Submitted = true;
        acquiredSwapChainTarget = false;
#endif
    }


#if DEBUG
    private void AssertTextureIsNotNull(ColorTargetInfo info) 
    {
        if (info.Texture == null || info.Texture.Handle == IntPtr.Zero) 
        {
            throw new InvalidOperationException("Render Target or a Target Texture in color attachment must not be null.");
        }
    }

    private void AssertColorIsRenderTarget(ColorTargetInfo info) 
    {
        if ((info.Texture.UsageFlags & TextureUsageFlags.ColorTarget) == 0) 
        {
            throw new InvalidOperationException("Render pass color attachment must be a Render Target or has a TextureUsageFlags.ColorTarget");
        }
    }

    private void AssertIsPassActive(string passName, bool pass) 
    {
        if (!pass) 
        {
            throw new Exception($"{passName} pass is currently not active.");
        }
    }
    private void AssertNoAnyPassActive() 
    {
        if (renderPassActive) 
        {
            throw new Exception("Render pass is currently in used.");
        }
        if (copyPassActive) 
        {
            throw new Exception("Copy pass is currently in used.");
        }
        if (computePassActive) 
        {
            throw new Exception("Compute pass is currently in used.");
        }
    }

    private void AssertNotSubmitted() 
    {
        if (Submitted) 
        {
            throw new Exception("This command buffer has already been submitted.");
        }
    }
#endif
}
