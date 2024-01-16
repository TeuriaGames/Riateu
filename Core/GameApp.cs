using System;
using MoonWorks;
using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu;

public abstract class GameApp : Game
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Scene nextScene;

    public Scene Scene 
    { 
        get => scene;
        set 
        {
            nextScene = value;
        }
    }
    private Scene scene;

    private Batch batch;

    protected GameApp(WindowCreateInfo windowCreateInfo, FrameLimiterSettings frameLimiterSettings, int targetTimestep = 60, bool debugMode = false) : base(windowCreateInfo, frameLimiterSettings, targetTimestep, debugMode)
    {
        Width = (int)windowCreateInfo.WindowWidth;
        Height = (int)windowCreateInfo.WindowHeight;
        GameContext.Init(GraphicsDevice, MainWindow);
        LoadContent();
        Initialize();
        batch = new Batch(GraphicsDevice, Width, Height);
    }

    public abstract void Initialize();
    public virtual void LoadContent() {}


    protected override void Draw(double alpha)
    {
        CommandBuffer cmdBuf = GraphicsDevice.AcquireCommandBuffer();
        Texture backbuffer =  cmdBuf.AcquireSwapchainTexture(MainWindow);

        scene.InternalBeforeDraw(ref cmdBuf, batch);
        if (backbuffer != null) 
        {
            scene.InternalDraw(ref cmdBuf, backbuffer, batch);
        }
        scene.InternalAfterDraw(ref cmdBuf, batch);

        GraphicsDevice.Submit(cmdBuf);
    }


    protected override void Update(TimeSpan delta)
    {
        Time.Update(delta);
        if (scene == null || (scene != nextScene)) 
        {
            scene?.End();
            scene = nextScene;
            scene.Begin();
        }
        scene.InternalUpdate(Time.Delta);
    }
}