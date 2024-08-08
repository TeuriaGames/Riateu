using System;
using ImGuiNET;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;
using Riateu.Inputs;

public class ContentWindow : GameLoop
{
    private ImGuiRenderer renderer;
    private AssetsContainer assetsContainer;


    public ContentWindow(GameApp gameApp) : base(gameApp)
    {
        renderer = new ImGuiRenderer(gameApp.GraphicsDevice, gameApp.MainWindow, 1024, 640, Init);

        assetsContainer = new AssetsContainer(OnAssetSelected);
    }

    private void Init(ImGuiIOPtr ptr) 
    {
        ptr.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
    }


    public override void Begin() {}

    public override void End() {}

    public override void Update(double delta)
    {
        renderer.Update(Input.Device, UIBuild);
    }

    public override void Render(RenderTarget backbuffer)
    {
        RenderPass renderPass = GraphicsDevice.BeginTarget(backbuffer, Color.Black, true);
        renderer.Render(renderPass);
        GraphicsDevice.EndTarget(renderPass);
    }

    private void UIBuild() 
    {
        SetupDockspace();
        // ImGui.ShowDemoWindow();

        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File")) 
        {
            if (ImGui.MenuItem("Exit")) 
            {
                GameInstance.Quit();
            }
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();

        assetsContainer.Draw();

        ImGui.Begin("Metadata");

        ImGui.End();

        ImGui.End(); // Dockspace
    }

    private void OnAssetSelected(string path) 
    {
        Console.WriteLine(path);
    }


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
    }
}
