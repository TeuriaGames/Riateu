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
        uint w = 0, h = 0;
        IntPtr texPtr = SDL.SDL_AcquireGPUSwapchainTexture(Handle, window.Handle, ref w, ref h);

        if (texPtr == IntPtr.Zero) 
        {
            return null;
        }

        window.SwapchainTarget.Handle = texPtr;
        window.SwapchainTarget.Width = w;
        window.SwapchainTarget.Height = h;
        window.SwapchainTarget.Format = window.SwapchainFormat;

        return window.SwapchainTarget;
    }

    public void Blit(in TextureRegion source, in TextureRegion destination, Filter filter, bool cycle) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
#endif
        SDL.SDL_GPUBlitInfo info = new SDL.SDL_GPUBlitInfo 
        {
            source = source.ToSDLGpu(),
            destination = source.ToSDLGpu(),
            filter = (SDL.SDL_GPUFilter)filter,
            cycle = cycle
        };
        SDL.SDL_BlitGPUTexture(Handle, ref info);
    }

    public unsafe RenderPass BeginRenderPass(in ColorAttachmentInfo info) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);
        renderPassActive = true;
#endif
        SDL.SDL_GPUColorTargetInfo *infos = stackalloc SDL.SDL_GPUColorTargetInfo[1];
        infos[0] = info.ToSDLGpu();

        IntPtr pass = SDL.SDL_BeginGPURenderPass(Handle, ref infos[0], 1, (SDL.SDL_GPUDepthStencilTargetInfo*)IntPtr.Zero);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in ColorAttachmentInfo info, in ColorAttachmentInfo info2) 
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
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[2];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 2, (SDL.SDL_GPUDepthStencilAttachmentInfo*)IntPtr.Zero);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in ColorAttachmentInfo info, in ColorAttachmentInfo info2, in ColorAttachmentInfo info3) 
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
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[3];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 3, (SDL.SDL_GPUDepthStencilAttachmentInfo*)IntPtr.Zero);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in ColorAttachmentInfo info, in ColorAttachmentInfo info2, in ColorAttachmentInfo info3, in ColorAttachmentInfo info4) 
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
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[4];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();
        infos[3] = info3.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 4, (SDL.SDL_GPUDepthStencilAttachmentInfo*)IntPtr.Zero);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(Span<ColorAttachmentInfo> infoSpan) 
    {
        int length = infoSpan.Length;
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        for (int i = 0; i < length; i++) 
        {
            ColorAttachmentInfo info = infoSpan[i];
            AssertTextureIsNotNull(info);
            AssertColorIsRenderTarget(info);
        }

        renderPassActive = true;
#endif
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[length];
        for (int i = 0; i < length; i++) 
        {
            infos[i] = infoSpan[i].ToSDLGpu();
        }

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, (uint)length, (SDL.SDL_GPUDepthStencilAttachmentInfo*)IntPtr.Zero);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilAttachmentInfo depthStencilAttachment, in Span<ColorAttachmentInfo> infoSpan) 
    {
        int length = infoSpan.Length;
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();


        for (int i = 0; i < length; i++) 
        {
            ColorAttachmentInfo info = infoSpan[i];
            AssertTextureIsNotNull(info);
            AssertColorIsRenderTarget(info);
        }
        renderPassActive = true;
#endif
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[1];
        for (int i = 0; i < length; i++) 
        {
            infos[i] = infoSpan[i].ToSDLGpu();
        }

        Refresh.DepthStencilAttachmentInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 1, &dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilAttachmentInfo depthStencilAttachment, in ColorAttachmentInfo info) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();

        AssertTextureIsNotNull(info);
        AssertColorIsRenderTarget(info);
        renderPassActive = true;
#endif
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[1];
        infos[0] = info.ToSDLGpu();

        Refresh.DepthStencilAttachmentInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 1, &dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilAttachmentInfo depthStencilAttachment, in ColorAttachmentInfo info, in ColorAttachmentInfo info2) 
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
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[2];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();

        Refresh.DepthStencilAttachmentInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 2, &dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilAttachmentInfo depthStencilAttachment, in ColorAttachmentInfo info, in ColorAttachmentInfo info2, in ColorAttachmentInfo info3) 
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
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[3];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();
        Refresh.DepthStencilAttachmentInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 3, &dsa);
        RenderPass renderPass = PassPool<RenderPass>.Obtain(pass);
        return renderPass;
    }

    public unsafe RenderPass BeginRenderPass(in DepthStencilAttachmentInfo depthStencilAttachment, in ColorAttachmentInfo info, in ColorAttachmentInfo info2, in ColorAttachmentInfo info3, in ColorAttachmentInfo info4) 
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
        SDL.SDL_GPUColorAttachmentInfo* infos = stackalloc SDL.SDL_GPUColorAttachmentInfo[4];
        infos[0] = info.ToSDLGpu();
        infos[1] = info2.ToSDLGpu();
        infos[2] = info3.ToSDLGpu();
        infos[3] = info3.ToSDLGpu();

        Refresh.DepthStencilAttachmentInfo dsa = depthStencilAttachment.ToSDLGpu();

        IntPtr pass = SDL.SDL_GPURefresh_BeginRenderPass(Handle, infos, 3, &dsa);
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
        SDL.SDL_GPURefresh_EndRenderPass(renderPass.Handle);
        PassPool<RenderPass>.Release(renderPass);
    }

    public CopyPass BeginCopyPass() 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
        copyPassActive = true;
#endif
        IntPtr copyPassPtr = Refresh.Refresh_BeginCopyPass(Handle);

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
        Refresh.Refresh_EndCopyPass(copyPass.Handle);
        PassPool<CopyPass>.Release(copyPass);
    }

    public unsafe ComputePass BeginComputePass(in StorageBufferReadWriteBinding readWriteBinding) 
    {
#if DEBUG
        AssertNotSubmitted();
        AssertNoAnyPassActive();
        computePassActive = true;
#endif
        Refresh.StorageBufferReadWriteBinding storage = readWriteBinding.ToSDLGpu();
        IntPtr computePassPtr = Refresh.Refresh_BeginComputePass(Handle, (Refresh.StorageTextureReadWriteBinding*)nint.Zero, 0, &storage, 1);

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
        Refresh.StorageTextureReadWriteBinding storage = readWriteBinding.ToSDLGpu();
        IntPtr computePassPtr = Refresh.Refresh_BeginComputePass(Handle, &storage, 1, (Refresh.StorageBufferReadWriteBinding*)nint.Zero, 0);

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
        Refresh.StorageBufferReadWriteBinding bufStorage = buffer.ToSDLGpu();
        Refresh.StorageTextureReadWriteBinding texStorage = texture.ToSDLGpu();
        IntPtr computePassPtr = Refresh.Refresh_BeginComputePass(Handle, &texStorage, 1, &bufStorage, 1);

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
        Refresh.Refresh_EndComputePass(computePass.Handle);
        PassPool<ComputePass>.Release(computePass);
    }

	/// <summary>
	/// Pushes data to a vertex uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a render or compute pass.
	/// </summary>
	public unsafe void PushVertexUniformData(void* uniformsPtr, uint size, uint slot = 0) 
    {
		Refresh.Refresh_PushVertexUniformData(Handle, slot, (nint) uniformsPtr, size);
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
		Refresh.Refresh_PushFragmentUniformData(Handle, slot, (nint) uniformsPtr, size);
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
		Refresh.Refresh_PushComputeUniformData(Handle, slot, (nint) uniformsPtr, size);
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
        Handle = SDL.SDL_GPURefresh_AcquireCommandBuffer(device.Handle);
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
    private void AssertTextureIsNotNull(ColorAttachmentInfo info) 
    {
        if (info.TextureSlice.Texture == null || info.TextureSlice.Texture.Handle == IntPtr.Zero) 
        {
            throw new InvalidOperationException("Render Target or a Target Texture in color attachment must not be null.");
        }
    }

    private void AssertColorIsRenderTarget(ColorAttachmentInfo info) 
    {
        if ((info.TextureSlice.Texture.UsageFlags & TextureUsageFlags.ColorTarget) == 0) 
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
