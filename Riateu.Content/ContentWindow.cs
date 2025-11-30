using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using Riateu.Graphics;
using Riateu.ImGuiRend;
using Riateu.Inputs;
using SDL3;

namespace Riateu.Content.App;

public class ContentWindow : GameLoop
{
    private ImGuiRenderer renderer;
    private AssetsContainer assetsContainer;
    private ImageRenderer imageRenderer;
    private ImageCache imageCache;
    private Batch batch;

    public string SelectedProject { get; private set; }


    public ContentWindow(GameApp gameApp) : base(gameApp)
    {
        renderer = new ImGuiRenderer(gameApp.GraphicsDevice, gameApp.MainWindow, 1024, 640, Init);
        batch = new Batch(gameApp.GraphicsDevice, 1024, 768);
        assetsContainer = new AssetsContainer(this);
        assetsContainer.OnAssetSelected = OnAssetSelected;
        assetsContainer.OnSelectProject = OnOpenProject;
        imageRenderer = new ImageRenderer(gameApp.GraphicsDevice, batch, renderer);
        imageCache = new ImageCache(gameApp.GraphicsDevice);
    }

    private unsafe void Init(ImGuiIOPtr ptr) 
    {
        ptr.ConfigFlags |= ImGuiConfigFlags.DockingEnable;


        ImFontConfig* config = ImGuiNative.ImFontConfig_ImFontConfig();
        config->MergeMode = 1;
        config->PixelSnapH = 1;
        config->FontDataOwnedByAtlas = 0;

        config->GlyphMaxAdvanceX = float.MaxValue;
        config->RasterizerMultiply = 1.0f;
        config->OversampleH = 2;
        config->OversampleV = 1;

        ushort* ranges = stackalloc ushort[3];
        ranges[0] = FA6.IconMin;
        ranges[1] = FA6.IconMax;
        ranges[2] = 0;


        byte *iconFontRange = (byte*)NativeMemory.Alloc(6);
        NativeMemory.Copy(ranges, iconFontRange, 6);
        config->GlyphRanges = (ushort*)iconFontRange;
        FA6.IconFontRanges = (IntPtr)iconFontRange;

        byte[] fontDataBuffer = Convert.FromBase64String(FA6.IconFontData);

        fixed (byte *buffer = &fontDataBuffer[0])
        {
            var fontPtr = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(new IntPtr(buffer), fontDataBuffer.Length, 11, config, FA6.IconFontRanges);
        }

        ImGuiNative.ImFontConfig_destroy(config);
    }


    public override void Begin() {}

    public override unsafe void End() 
    {
        NativeMemory.Free((void*)FA6.IconFontRanges);
    }

    public override void Update(double delta)
    {
        renderer.Update(Input.Device, UIBuild);
        imageRenderer.Update();
    }

    public override void Render(RenderTarget backbuffer)
    {
        imageRenderer.Render();

        RenderPass renderPass = GraphicsDevice.BeginTarget(backbuffer, Color.Black, true);
        renderer.Render(renderPass);
        GraphicsDevice.EndTarget(renderPass);
    }

    private void UIBuild() 
    {
        SetupDockspace();

        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File")) 
        {
            if (ImGui.MenuItem("Open")) 
            {
                OnOpenProject();
            }
            if (ImGui.MenuItem("Exit")) 
            {
                GameInstance.Quit();
            }
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();

        assetsContainer.Draw();

        ImGui.Begin($"{FA6.Table} Importer");
        imageRenderer.Draw();
        ImGui.End();

        ImGui.End(); // Dockspace
    }

    private void OnOpenProject() 
    {
        SDL.SDL_ShowOpenFolderDialog(OnDialogOpen, IntPtr.Zero, IntPtr.Zero, "./", false);
    }

    private unsafe void OnDialogOpen(IntPtr userdata, IntPtr filelist, int filter)
    {
        if (filelist == IntPtr.Zero)
        {
            return;
        }
        if (*(byte*)filelist == IntPtr.Zero) 
        {
            return;
        }
        byte **files = (byte**)filelist;
        byte *ptr = files[0];
        int count = 0;
        while (*ptr != 0)
        {
            ptr++;
            count++;
        }

        if (count <= 0)
        {
            return;
        }
        string folder = Encoding.UTF8.GetString(files[0], count);
        SelectedProject = folder;
    }

    private void OnAssetSelected(string path) 
    {
        if (path.EndsWith("png")) 
        {
            imageRenderer.SetActiveTexture(imageCache.LoadImage(path));
        }
    }


    private bool firstLoop = true;
    private void SetupDockspace() 
    {
        var windowFlags = 
            ImGuiWindowFlags.MenuBar
            | ImGuiWindowFlags.NoDocking;
        
        ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(1024, 640));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        windowFlags |= ImGuiWindowFlags.NoTitleBar 
            | ImGuiWindowFlags.NoCollapse 
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoNavFocus;

        bool dockSpaceTrue = true;
        ImGui.Begin("Dockspace", ref dockSpaceTrue, windowFlags); 
        ImGui.PopStyleVar(2);

        // Dockspace
        ImGuiIOPtr ioPtr = ImGui.GetIO();

        if ((ioPtr.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0) 
        {
            var dockspaceID = ImGui.GetID("MyDockSpace");
            ImGui.DockSpace(dockspaceID, System.Numerics.Vector2.Zero);
        }

        if (firstLoop) 
        {
            Vector2 workCenter = ImGui.GetMainViewport().GetWorkCenter();
            uint id = ImGui.GetID("MyDockSpace");
            DockNative.igDockBuilderRemoveNode(id);
            DockNative.igDockBuilderAddNode(id);

            Vector2 size = new Vector2(600, 300);
            Vector2 nodePos = new Vector2(workCenter.X - size.X * 0.5f, workCenter.Y - size.Y * 0.5f);

            // Set the size and position:
            DockNative.igDockBuilderSetNodeSize(id, size);
            DockNative.igDockBuilderSetNodePos(id, nodePos);

            uint dock1 = DockNative.igDockBuilderSplitNode(id, ImGuiDir.Left, 0.5f, out _, out id);
            uint dock2 = DockNative.igDockBuilderSplitNode(id, ImGuiDir.Right, 0.5f, out _, out id);

            DockNative.igDockBuilderDockWindow($"{FA6.Box} Assets", dock1);
            DockNative.igDockBuilderDockWindow($"{FA6.Table} Importer", dock2);

            DockNative.igDockBuilderFinish(id);
            firstLoop = false;
        }
    }
}
