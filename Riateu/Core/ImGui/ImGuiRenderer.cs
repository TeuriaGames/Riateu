using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Riateu.Graphics;
using Riateu.Inputs;
using Riateu.Misc;

namespace Riateu.ImGuiRend;

/// <summary>
/// A canvas renderer to render ImGui library.
/// </summary>
public class ImGuiRenderer 
{
    private Dictionary<nint, Texture> PtrMap = new();
    private GraphicsPipeline imGuiPipeline;
    private Shader imGuiShader;
    private Sampler imGuiSampler;
    private GraphicsDevice device;
    private uint vertexCount;
    private uint indexCount;
    private StructuredBuffer<Position2DTextureColorVertex> imGuiVertexBuffer;
    private StructuredBuffer<ushort> imGuiIndexBuffer;
    private TransferBuffer transferBuffer;

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
        this.device = device;
        IntPtr context = ImGui.CreateContext();

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
                        ColorAttachmentBlendState.NonPremultiplied
                    )
                ),
                DepthStencilState = DepthStencilState.Disable,
                PrimitiveType = PrimitiveType.TriangleList,
                RasterizerState = RasterizerState.CCW_CullNone,
                MultisampleState = MultisampleState.None,
                VertexShader = imGuiShader,
                FragmentShader = fragmentShader,
                VertexInputState = new VertexInputState(
                    VertexBufferDescription.Create<Position2DTextureColorVertex>(0),
                    Position2DTextureColorVertex.Attributes(0)
                )
            }
        );

        window.OnSizeChange += HandleSizeChanged;

        Keyboard.TextInput += c =>
        {
            if (c == '\t') { return; }
            io.AddInputCharacter(c);
        };

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        if (!OperatingSystem.IsWindows())
        {
            unsafe 
            {
                io.SetClipboardTextFn = (nint)Clipboard.SetFnPtr;
                io.GetClipboardTextFn = (nint)Clipboard.GetFnPtr;
            }
        }

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
    public void Update(InputDevice inputs, Action imGuiCallback)
    {
        var io = ImGui.GetIO();

        io.MousePos = new System.Numerics.Vector2(inputs.Mouse.X, inputs.Mouse.Y);
        io.MouseDown[0] = inputs.Mouse.LeftButton.IsDown;
        io.MouseDown[1] = inputs.Mouse.RightButton.IsDown;
        io.MouseDown[2] = inputs.Mouse.MiddleButton.IsDown;
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


        ImGui.NewFrame();
        imGuiCallback();
        ImGui.EndFrame();
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
        copyPass.UploadToBuffer(new TransferBufferLocation(transferBuffer, (uint)vertexSize), new BufferRegion(imGuiIndexBuffer, 0, (uint)indexSize), true);
        commandBuffer.EndCopyPass(copyPass);

        device.Submit(commandBuffer);
    }

    public void Render(RenderPass renderPass)
    {
        ImGui.Render();

        var io = ImGui.GetIO();
        var drawDataPtr = ImGui.GetDrawData();

        UpdateImGuiBuffers(drawDataPtr);
        CommandBuffer buffer = device.DeviceCommandBuffer();
        RenderCommandLists(buffer, renderPass, drawDataPtr, io);
    }

    private void RenderCommandLists(CommandBuffer buffer, RenderPass renderPass, ImDrawDataPtr drawDataPtr, ImGuiIOPtr ioPtr)
    {
        renderPass.BindGraphicsPipeline(imGuiPipeline);

        buffer.PushVertexUniformData(
            Matrix4x4.CreateOrthographicOffCenter(0, ioPtr.DisplaySize.X, ioPtr.DisplaySize.Y, 0, -1, 1)
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

                renderPass.BindFragmentSampler(
                    new TextureSamplerBinding(GetPointer(drawCmd.TextureId), imGuiSampler)
                );

                var width = drawCmd.ClipRect.Z - (int)drawCmd.ClipRect.X;
                var height = drawCmd.ClipRect.W - (int)drawCmd.ClipRect.Y;

                if (width <= 0 || height <= 0)
                {
                    continue;
                }

                int x = drawCmd.ClipRect.X < 0 ? 0 : (int)drawCmd.ClipRect.X;
                int y = drawCmd.ClipRect.Y < 0 ? 0 : (int)drawCmd.ClipRect.Y;

                renderPass.SetScissor(new Rectangle(x, y, (int)width, (int)height));

                renderPass.DrawIndexedPrimitives(
                    drawCmd.ElemCount,
                    1,
                    indexOffset,
                    vertexOffset,
                    0u
                );
                indexOffset += (uint)drawCmd.ElemCount;
            }

            vertexOffset += cmdList.VtxBuffer.Size;
        }
    }

    /// <summary>
    /// Destroy the ImGui context.
    /// </summary>
    public void Destroy()
    {
        ImGui.DestroyContext();
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
}
