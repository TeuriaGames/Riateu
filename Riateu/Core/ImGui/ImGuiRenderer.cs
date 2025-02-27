using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Riateu.Graphics;
using Riateu.Inputs;
using Riateu.Misc;
using SDL3;

namespace Riateu.ImGuiRend;

/// <summary>
/// A canvas renderer to render ImGui library.
/// </summary>
public class ImGuiRenderer 
{
    private int windowWidth;
    private int windowHeight;
    private Dictionary<nint, Texture> PtrMap = new();
    private GraphicsPipeline imGuiPipeline;
    private Shader imGuiShader;
    private Sampler imGuiSampler;
    private GraphicsDevice device;
    private uint vertexCount = 1;
    private uint indexCount = 1;
    private StructuredBuffer<Position2DTextureColorVertex> imGuiVertexBuffer;
    private StructuredBuffer<ushort> imGuiIndexBuffer;
    private TransferBuffer transferBuffer;
    private Window window;
    private ImGuiWindow imguiWindow;

    private readonly Platform_CreateWindow pCreateWindow;
    private readonly Platform_DestroyWindow pDestroyWindow;
    private readonly Platform_GetWindowPos pGetWindowPos;
    private readonly Platform_ShowWindow pShowWindow;
    private readonly Platform_SetWindowPos pSetWindowPos;
    private readonly Platform_SetWindowSize pSetWindowSize;
    private readonly Platform_GetWindowSize pGetWindowSize;
    private readonly Platform_SetWindowFocus pSetWindowFocus;
    private readonly Platform_GetWindowFocus pGetWindowFocus;
    private readonly Platform_GetWindowMinimized pGetWindowMinimized;
    private readonly Platform_SetWindowTitle pSetWindowTitle;

    private void CreateWindow(ImGuiViewportPtr viewportPtr)
    {
        ImGuiWindow window = new ImGuiWindow(device, viewportPtr);
    }

    private void DestroyWindow(ImGuiViewportPtr viewportPtr)
    {
        if (viewportPtr.PlatformUserData != IntPtr.Zero)
        {
            ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(viewportPtr.PlatformUserData).Target;
            window.Dispose();

            viewportPtr.PlatformUserData = IntPtr.Zero;
        }
    }

    private void ShowWindow(ImGuiViewportPtr vp)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        window.Window.Show();
        SDL.SDL_SetWindowPosition(window.Window.Handle, (int)vp.Pos.X, (int)vp.Pos.Y);
    }

    private unsafe void GetWindowPos(ImGuiViewportPtr vp, Vector2* outPos)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        int x = 0; 
        int y = 0;
        SDL.SDL_GetWindowPosition(window.Window.Handle, out x, out y);
        *outPos = new Vector2(x, y);
    }

    private void SetWindowPos(ImGuiViewportPtr vp, Vector2 pos)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        SDL.SDL_SetWindowPosition(window.Window.Handle, (int)pos.X, (int)pos.Y);
    }

    private void SetWindowSize(ImGuiViewportPtr vp, Vector2 size)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        window.Window.SetWindowSizeRelative((uint)size.X, (uint)size.Y);
    }

    private unsafe void GetWindowSize(ImGuiViewportPtr vp, Vector2* outSize)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        *outSize = new Vector2(window.Window.Width, window.Window.Height);
    }

    private void SetWindowFocus(ImGuiViewportPtr vp)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        SDL.SDL_RaiseWindow(window.Window.Handle);
    }

    private byte GetWindowFocus(ImGuiViewportPtr vp)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        SDL.SDL_WindowFlags flags = SDL.SDL_GetWindowFlags(window.Window.Handle);
        return (flags & SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0 ? (byte)1 : (byte)0;
    }

    private byte GetWindowMinimized(ImGuiViewportPtr vp)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        SDL.SDL_WindowFlags flags = SDL.SDL_GetWindowFlags(window.Window.Handle);
        return (flags & SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0 ? (byte)1 : (byte)0;
    }

    private unsafe void SetWindowTitle(ImGuiViewportPtr vp, IntPtr title)
    {
        ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
        byte* titlePtr = (byte*)title;
        int count = 0;
        while (titlePtr[count] != 0)
        {
            count += 1;
        }

        window.Window.Title = System.Text.Encoding.ASCII.GetString(titlePtr, count);
    }

    /// <summary>
    /// A initilization for the ImGui renderer and to create its context.
    /// </summary>
    /// <param name="device">An application device</param>
    /// <param name="window">An application window</param>
    /// <param name="width">A width of the canvas</param>
    /// <param name="height">A height of the canvas</param>
    /// <param name="onInit">Called before building the font</param>
    public ImGuiRenderer(GraphicsDevice device, Window window, int width, int height, Action<ImGuiIOPtr> onInit = null)
    {
        windowWidth = width;
        windowHeight = height;
        this.window = window;
        this.device = device;
        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(width, height);
        io.DisplayFramebufferScale = System.Numerics.Vector2.One;
        imGuiShader = Resources.GetShader(device, Resources.ImGuiShader, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Vertex,
            ShaderFormat = GraphicsDevice.BackendShaderFormat,
            UniformBufferCount = 1
        });
        imGuiSampler = new Sampler(device, SamplerCreateInfo.PointClamp);

        var fragmentShader = Resources.GetShader(device, Resources.Texture, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = GraphicsDevice.BackendShaderFormat,
            SamplerCount = 1
        });

        imGuiPipeline = new GraphicsPipeline(
            device,
            new GraphicsPipelineCreateInfo
            {
                AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                    new ColorAttachmentDescription(
                        window.SwapchainFormat,
                        ColorTargetBlendState.NonPremultiplied
                    )
                ),
                DepthStencilState = DepthStencilState.Disable,
                PrimitiveType = PrimitiveType.TriangleList,
                RasterizerState = RasterizerState.CW_CullNone,
                MultisampleState = MultisampleState.None,
                VertexShader = imGuiShader,
                FragmentShader = fragmentShader,
                VertexInputState = new VertexInputState(
                    VertexBufferDescription.Create<Position2DTextureColorVertex>(0),
                    Position2DTextureColorVertex.Attributes(0)
                )
            }
        );

        window.Resized += HandleSizeChanged;

        Keyboard.TextInput += c =>
        {
            if (c == '\t') { return; }
            io.AddInputCharacter(c);
        };

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        var platformIO = ImGui.GetPlatformIO();
        ImGuiViewportPtr ptr = platformIO.Viewports[0];
        ptr.PlatformHandle = window.Handle;
        imguiWindow = new ImGuiWindow(device, ptr, window);
        unsafe {
            pCreateWindow = CreateWindow;
            pDestroyWindow = DestroyWindow;
            pGetWindowPos = GetWindowPos;
            pShowWindow = ShowWindow;
            pSetWindowPos = SetWindowPos;
            pSetWindowSize = SetWindowSize;
            pGetWindowSize = GetWindowSize;
            pSetWindowFocus = SetWindowFocus;
            pGetWindowFocus = GetWindowFocus;
            pGetWindowMinimized = GetWindowMinimized;
            pSetWindowTitle = SetWindowTitle;
        }

        platformIO.Platform_CreateWindow = Marshal.GetFunctionPointerForDelegate(pCreateWindow);
        platformIO.Platform_DestroyWindow = Marshal.GetFunctionPointerForDelegate(pDestroyWindow);
        platformIO.Platform_ShowWindow = Marshal.GetFunctionPointerForDelegate(pShowWindow);
        platformIO.Platform_SetWindowPos = Marshal.GetFunctionPointerForDelegate(pSetWindowPos);
        platformIO.Platform_SetWindowSize = Marshal.GetFunctionPointerForDelegate(pSetWindowSize);
        platformIO.Platform_SetWindowFocus = Marshal.GetFunctionPointerForDelegate(pSetWindowFocus);
        platformIO.Platform_GetWindowFocus = Marshal.GetFunctionPointerForDelegate(pGetWindowFocus);
        platformIO.Platform_GetWindowMinimized = Marshal.GetFunctionPointerForDelegate(pGetWindowMinimized);
        platformIO.Platform_SetWindowTitle = Marshal.GetFunctionPointerForDelegate(pSetWindowTitle);

        unsafe {
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowPos(platformIO.NativePtr, Marshal.GetFunctionPointerForDelegate(pGetWindowPos));
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowSize(platformIO.NativePtr, Marshal.GetFunctionPointerForDelegate(pGetWindowSize));
        }


        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
        io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;

        ImGui.SetCurrentContext(context);
        io.Fonts.AddFontDefault();
        onInit?.Invoke(io);

        BuildFontAtlas();

        imGuiVertexBuffer = new StructuredBuffer<Position2DTextureColorVertex>(
            device,
            BufferUsageFlags.Vertex,
            vertexCount
        );

        imGuiIndexBuffer = new StructuredBuffer<ushort>(
            device,
            BufferUsageFlags.Index,
            indexCount
        );
    }

    private void HandleSizeChanged(uint width, uint height)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(width, height);
    }

    /// <summary>
    /// A method that updates ImGui inputs and for drawing.
    /// </summary>
    /// <param name="inputs">An application input device</param>
    /// <param name="imGuiCallback">A callback used for rendering</param>
    public unsafe void Update(InputDevice inputs, Action imGuiCallback)
    {
        var io = ImGui.GetIO();
        float x = 0;
        float y = 0;
        var buttons = (uint)SDL.SDL_GetGlobalMouseState(out x, out y);

        io.MousePos = new System.Numerics.Vector2(x, y);
        io.MouseDown[0] = (buttons & 0b0001) != 0;
        io.MouseDown[1] = (buttons & 0b0010) != 0;
        io.MouseDown[2] = (buttons & 0b0100) != 0;
        io.MouseWheel = inputs.Mouse.WheelY;

        io.AddKeyEvent(ImGuiKey.A, inputs.Keyboard.IsDown(KeyCode.A));
        io.AddKeyEvent(ImGuiKey.Z, inputs.Keyboard.IsDown(KeyCode.Z));
        io.AddKeyEvent(ImGuiKey.Y, inputs.Keyboard.IsDown(KeyCode.Y));
        io.AddKeyEvent(ImGuiKey.X, inputs.Keyboard.IsDown(KeyCode.X));
        io.AddKeyEvent(ImGuiKey.C, inputs.Keyboard.IsDown(KeyCode.C));
        io.AddKeyEvent(ImGuiKey.V, inputs.Keyboard.IsDown(KeyCode.V));

        io.AddKeyEvent(ImGuiKey.Tab, inputs.Keyboard.IsDown(KeyCode.Tab));
        io.AddKeyEvent(ImGuiKey.LeftArrow, inputs.Keyboard.IsDown(KeyCode.Left));
        io.AddKeyEvent(ImGuiKey.RightArrow, inputs.Keyboard.IsDown(KeyCode.Right));
        io.AddKeyEvent(ImGuiKey.UpArrow, inputs.Keyboard.IsDown(KeyCode.Up));
        io.AddKeyEvent(ImGuiKey.DownArrow, inputs.Keyboard.IsDown(KeyCode.Down));
        io.AddKeyEvent(ImGuiKey.Enter, inputs.Keyboard.IsDown(KeyCode.Return));
        io.AddKeyEvent(ImGuiKey.Escape, inputs.Keyboard.IsDown(KeyCode.Escape));
        io.AddKeyEvent(ImGuiKey.Delete, inputs.Keyboard.IsDown(KeyCode.Delete));
        io.AddKeyEvent(ImGuiKey.Backspace, inputs.Keyboard.IsDown(KeyCode.Backspace));
        io.AddKeyEvent(ImGuiKey.Home, inputs.Keyboard.IsDown(KeyCode.Home));
        io.AddKeyEvent(ImGuiKey.End, inputs.Keyboard.IsDown(KeyCode.End));
        io.AddKeyEvent(ImGuiKey.PageDown, inputs.Keyboard.IsDown(KeyCode.PageDown));
        io.AddKeyEvent(ImGuiKey.PageUp, inputs.Keyboard.IsDown(KeyCode.PageUp));
        io.AddKeyEvent(ImGuiKey.Insert, inputs.Keyboard.IsDown(KeyCode.Insert));

        io.AddKeyEvent(ImGuiKey.ModCtrl, inputs.Keyboard.IsDown(KeyCode.LeftControl) || inputs.Keyboard.IsDown(KeyCode.RightControl));
        io.AddKeyEvent(ImGuiKey.ModShift, inputs.Keyboard.IsDown(KeyCode.LeftShift) || inputs.Keyboard.IsDown(KeyCode.RightShift));
        io.AddKeyEvent(ImGuiKey.ModAlt, inputs.Keyboard.IsDown(KeyCode.LeftAlt) || inputs.Keyboard.IsDown(KeyCode.RightAlt));
        io.AddKeyEvent(ImGuiKey.ModSuper, inputs.Keyboard.IsDown(KeyCode.LeftMeta) || inputs.Keyboard.IsDown(KeyCode.RightMeta));

        if (io.WantCaptureKeyboard) 
        {
            SDL.SDL_StartTextInput(window.Handle);
        } 
        else 
        {
            SDL.SDL_StopTextInput(window.Handle);
        }

        SetPerFrameImGuiData(Time.Delta);
        UpdateMonitors();

        ImGui.NewFrame();
        imGuiCallback();
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            window.Width / 1,
            window.Height / 1);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.

        int x = 0;
        int y = 0;
        SDL.SDL_GetWindowPosition(window.Handle, out x, out y);

        ImGui.GetPlatformIO().Viewports[0].Pos = new Vector2(x, y);
        ImGui.GetPlatformIO().Viewports[0].Size = new Vector2(window.Width, window.Height);
    }


    private unsafe void UpdateMonitors()
    {
        ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
        NativeMemory.Free(platformIO.NativePtr->Monitors.Data.ToPointer());
        int numMonitors = 0;
        SDL.SDL_GetDisplays(out numMonitors);
        void *data = NativeMemory.Alloc((nuint)(Unsafe.SizeOf<ImGuiPlatformMonitor>() * numMonitors));
        platformIO.NativePtr->Monitors = new ImVector(numMonitors, numMonitors, (IntPtr)data);
        for (int i = 1; i <= numMonitors; i++)
        {
            SDL.SDL_Rect r = default;
            SDL.SDL_GetDisplayUsableBounds((uint)i, out r);
            ImGuiPlatformMonitorPtr monitor = platformIO.Monitors[i - 1];
            monitor.DpiScale = 1f;
            monitor.MainPos = new Vector2(r.x, r.y);
            monitor.MainSize = new Vector2(r.w, r.h);
            monitor.WorkPos = new Vector2(r.x, r.y);
            monitor.WorkSize = new Vector2(r.w, r.h);
        }
    }

    private unsafe void UpdateImGuiBuffers(ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.TotalVtxCount == 0) { return; }

        bool needNewTransferBuffer = false;

        var commandBuffer = device.AcquireCommandBuffer();

        if (drawDataPtr.TotalVtxCount > vertexCount)
        {
            imGuiVertexBuffer?.Dispose();

            vertexCount = (uint)(drawDataPtr.TotalVtxCount * 1.5f);
            imGuiVertexBuffer = new StructuredBuffer<Position2DTextureColorVertex>(
                device,
                BufferUsageFlags.Vertex,
                vertexCount
            );
            needNewTransferBuffer = true;
        }

        if (drawDataPtr.TotalIdxCount > indexCount)
        {
            imGuiIndexBuffer?.Dispose();

            indexCount = (uint)(drawDataPtr.TotalIdxCount * 1.5f);
            imGuiIndexBuffer = new StructuredBuffer<ushort>(
                device,
                BufferUsageFlags.Index,
                indexCount
            );
            needNewTransferBuffer = true;
        }

        if (needNewTransferBuffer)
        {
            transferBuffer?.Dispose();
            transferBuffer = new TransferBuffer(device, TransferBufferUsage.Upload, imGuiVertexBuffer.Size + imGuiIndexBuffer.Size);
        }

        int vertexSize = 0;
        int indexSize = 0;
        int indexOffset = drawDataPtr.TotalVtxCount * sizeof(Position2DTextureColorVertex);

        var vertexIndexData = transferBuffer.Map(true, 0);
        fixed (byte *ptr = vertexIndexData) 
        {
            for (var n = 0; n < drawDataPtr.CmdListsCount; n += 1)
            {
                var cmdList = drawDataPtr.CmdLists[n];
                int size = cmdList.VtxBuffer.Size * sizeof(Position2DTextureColorVertex);
                NativeMemory.Copy((void*)cmdList.VtxBuffer.Data, &ptr[vertexSize], (nuint)size);
                vertexSize += size;

                size = cmdList.IdxBuffer.Size * sizeof(ushort);
                NativeMemory.Copy(
                    (void*)cmdList.IdxBuffer.Data, 
                    &ptr[indexOffset + indexSize], 
                    (nuint)size
                );
                indexSize += size;
            }
        }

        transferBuffer.Unmap();

        CopyPass copyPass = commandBuffer.BeginCopyPass();
        copyPass.UploadToBuffer(new TransferBufferLocation(transferBuffer, 0), new BufferRegion(imGuiVertexBuffer, 0, (uint)vertexSize), true);
        copyPass.UploadToBuffer(new TransferBufferLocation(transferBuffer, (uint)indexOffset), new BufferRegion(imGuiIndexBuffer, 0, (uint)indexSize), true);
        commandBuffer.EndCopyPass(copyPass);

        device.Submit(commandBuffer);
    }

    public void Render(CommandBuffer buffer, RenderPass renderPass)
    {
        ImGui.Render();

        var io = ImGui.GetIO();
        var drawDataPtr = ImGui.GetDrawData();

        UpdateImGuiBuffers(drawDataPtr);
        RenderCommandLists(buffer, renderPass, drawDataPtr);
        
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) == 0) 
        {
            return;
        }

        ImGui.UpdatePlatformWindows();
        var platformIO = ImGui.GetPlatformIO();
        for (int i = 1; i < platformIO.Viewports.Size; i++) 
        {
            var viewportPtr = platformIO.Viewports[i];
            ImGuiWindow window = (ImGuiWindow)GCHandle.FromIntPtr(viewportPtr.PlatformUserData).Target;
            if (!window.Window.Claimed) 
            {
                continue;
            }

            UpdateImGuiBuffers(viewportPtr.DrawData);
            var cmdBuffer = window.Device.AcquireCommandBuffer();

            var swapchainTarget = cmdBuffer.AcquireSwapchainTarget(window.Window);

            if (swapchainTarget != null) 
            {
                var viewportRenderPass = cmdBuffer.BeginRenderPass(new ColorTargetInfo(swapchainTarget, Color.Black, true));
                RenderCommandLists(cmdBuffer, viewportRenderPass, viewportPtr.DrawData);
                cmdBuffer.EndRenderPass(viewportRenderPass);
            }

            window.Device.Submit(cmdBuffer);
        }
    }

    private void RenderCommandLists(CommandBuffer buffer, RenderPass renderPass, ImDrawDataPtr drawDataPtr)
    {
        renderPass.BindGraphicsPipeline(imGuiPipeline);
        var pos = drawDataPtr.DisplayPos;

        buffer.PushVertexUniformData(
            Matrix4x4.CreateOrthographicOffCenter(pos.X, pos.X + drawDataPtr.DisplaySize.X, pos.Y + drawDataPtr.DisplaySize.Y, pos.Y, -1, 1)
        );

        renderPass.BindVertexBuffer(imGuiVertexBuffer);
        renderPass.BindIndexBuffer(imGuiIndexBuffer, IndexElementSize.SixteenBit);

        int vertexOffset = 0;
        uint indexOffset = 0;

        for (int n = 0; n < drawDataPtr.CmdListsCount; n += 1)
        {
            var cmdList = drawDataPtr.CmdLists[n];


            for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex += 1)
            {
                var drawCmd = cmdList.CmdBuffer[cmdIndex];

                var width = drawCmd.ClipRect.Z - (int)drawCmd.ClipRect.X;
                var height = drawCmd.ClipRect.W - (int)drawCmd.ClipRect.Y;

                if (width <= 0 || height <= 0)
                {
                    continue;
                }

                int x = drawCmd.ClipRect.X - pos.X < 0 ? 0 : (int)(drawCmd.ClipRect.X - pos.X);
                int y = drawCmd.ClipRect.Y - pos.Y < 0 ? 0 : (int)(drawCmd.ClipRect.Y - pos.Y);

                renderPass.BindFragmentSampler(
                    new TextureSamplerBinding(GetPointer(drawCmd.TextureId), imGuiSampler)
                );
                renderPass.SetScissor(new Rectangle(x, y, (int)width, (int)height));

                renderPass.DrawIndexedPrimitives(
                    drawCmd.ElemCount,
                    1,
                    indexOffset + drawCmd.IdxOffset,
                    vertexOffset,
                    0u
                );
            }

            indexOffset += (uint)cmdList.IdxBuffer.Size;
            vertexOffset += cmdList.VtxBuffer.Size;
        }
    }

    /// <summary>
    /// Destroy the ImGui context.
    /// </summary>
    public void Destroy()
    {
        DeinitMultiViewport();
        ImGui.DestroyContext();
        imGuiPipeline.Dispose();
        imGuiVertexBuffer.Dispose();
        imGuiIndexBuffer.Dispose();
        imGuiSampler.Dispose();
        transferBuffer.Dispose();
    }

    private unsafe void BuildFontAtlas()
    {
        var cmdBuf = device.AcquireCommandBuffer();

        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(
            out nint pixelData,
            out int width,
            out int height,
            out int bytesPerPixel
        );


        using var uploader = new ResourceUploader(device);
        var fontTexture = uploader.CreateTexture2D(new Span<byte>((void*)pixelData, width * height * bytesPerPixel), (uint)width, (uint)height);

        uploader.Upload();

        io.Fonts.SetTexID(fontTexture.Handle);
        io.Fonts.ClearTexData();

        BindTexture(fontTexture);
    }

    /// <summary>
    /// Bind a texture as an ImGui texture.
    /// </summary>
    /// <param name="texture">A texture to bind</param>
    /// <returns>A pointer to the bound texture</returns>
    public IntPtr BindTexture(Texture texture)
    {
        if (!PtrMap.ContainsKey(texture.Handle))
        {
            PtrMap.Add(texture.Handle, texture);
        }

        return texture.Handle;
    }

    /// <summary>
    /// Unbind the texture.
    /// </summary>
    /// <param name="ptr">A pointer to the bound texture</param>
    public void UnbindTexture(IntPtr ptr)
    {
        if (!PtrMap.ContainsKey(ptr))
        {
            PtrMap.Remove(ptr);
        }
    }

    /// <summary>
    /// Get a texture base on the bound texture pointer.
    /// </summary>
    /// <param name="ptr">A pointer to the bound texture</param>
    /// <returns>A texture from the pointer</returns>
    public Texture GetPointer(IntPtr ptr)
    {
        if (!PtrMap.ContainsKey(ptr))
        {
            return null;
        }

        var texture = PtrMap[ptr];

        return texture;
    }

    public void DeinitMultiViewport() 
    {
        ImGui.DestroyPlatformWindows();
    }
}