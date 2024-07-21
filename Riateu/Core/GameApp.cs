using System;
using MoonWorks;
using MoonWorks.Graphics;
using Riateu.Content;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// The main class entry point for your game. It handles the content, initialization,
/// update loop, and drawing.
/// </summary>
public abstract class GameApp : Game
{
    /// <summary>
    /// A window width of the application.
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// A window height of the application.
    /// </summary>
    public int Height { get; private set; }

    private GameLoop nextScene;

    /// <summary>
    /// A current scene that is running. Note that if you change this, the scene won't
    /// actually changed yet until next frame started.
    /// </summary>
    public GameLoop Scene
    {
        get => scene;
        set
        {
            nextScene = value;
        }
    }

    public string AssetPath = "Assets";
    private GameLoop scene;

    public AssetStorage Assets;

    internal static GameApp Instance;


    /// <summary>
    /// A constructor use for initializng the application.
    /// </summary>
    /// <param name="title">A title of the window</param>
    /// <param name="width">A width of the window</param>
    /// <param name="height">A height of the window</param>
    /// <param name="screenMode">The screen mode for the window</param>
    /// <param name="debugMode">Enable or disable debug mode, use for debugging graphics</param>
    protected GameApp(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false)
        : this(
            new WindowCreateInfo(title, width, height, screenMode),
            new FrameLimiterSettings(FrameLimiterMode.Capped, 60)
        )
    {

    }

    /// <summary>
    /// A constructor use for initializng the application.
    /// </summary>
    /// <param name="windowCreateInfo">An info for creating window</param>
    /// <param name="frameLimiterSettings">A settings to cap the frame</param>
    /// <param name="targetTimestep">The maximum fps timestep</param>
    /// <param name="debugMode">Enable or disable debug mode, use for debugging graphics</param>
    protected GameApp(WindowCreateInfo windowCreateInfo, FrameLimiterSettings frameLimiterSettings, int targetTimestep = 60, bool debugMode = false)
        : base(windowCreateInfo, SwapchainComposition.SDR, PresentMode.VSync, frameLimiterSettings,
#if D3D11
        BackendFlags.D3D11,
#elif Metal
        BackendFlags.Metal,
#elif Vulkan
        BackendFlags.Vulkan,
#endif
        targetTimestep, debugMode)
    {
        Instance = this;
        Width = (int)windowCreateInfo.WindowWidth;
        Height = (int)windowCreateInfo.WindowHeight;
        GameContext.Init(GraphicsDevice, MainWindow);
        Input.Initialize(Inputs);
        Assets = new AssetStorage(AssetPath);
        ResourceUploader uploader = new ResourceUploader(GraphicsDevice);
        Assets.StartContext(uploader);
        LoadContent(Assets);
        Assets.EndContext();
        Initialize();
    }

    /// <summary>
    /// A method for loading content. You can freely acquire and submit the
    /// <see cref="MoonWorks.Graphics.CommandBuffer" /> here.
    /// </summary>
    public virtual void LoadContent(AssetStorage storage) {}

    /// <summary>
    /// A method to also initialize your other resources, and to set your scene.
    /// </summary>
    public abstract void Initialize();

    private void InternalUpdate(TimeSpan delta) 
    {
        Time.Update(delta);
        Input.Update();
        if (scene == null || (scene != nextScene))
        {
            scene?.End();
            scene = nextScene;
            scene.Begin();
        }

        scene.Update(Time.Delta);
    }
    private void InternalDraw(double alpha) 
    {
        CommandBuffer cmdBuf = GraphicsExecutor.Acquire(GraphicsDevice);
        Texture backbuffer = cmdBuf.AcquireSwapchainTexture(MainWindow);
        if (backbuffer != null) 
        {
            scene.Render(new BackbufferTarget(backbuffer));
        }

        Time.Draw(alpha);
        GraphicsDevice.Submit(cmdBuf);
    }
    protected override sealed void Draw(double alpha) 
    { 
        InternalDraw(alpha);
    }
    
    protected override sealed void Update(TimeSpan delta) 
    { 
        InternalUpdate(delta);
    }
}
