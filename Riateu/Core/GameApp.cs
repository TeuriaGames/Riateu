using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Riateu.Audios;
using Riateu.Content;
using Riateu.Graphics;
using Riateu.Inputs;
using SDL2;

namespace Riateu;

/// <summary>
/// The main class entry point for your game. It handles the content, initialization,
/// update loop, and drawing.
/// </summary>
public abstract class GameApp 
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

    public Window MainWindow { get; internal set; }
    public GraphicsDevice GraphicsDevice { get; internal set; }
    public InputDevice InputDevice { get; internal set; }
    public AudioDevice AudioDevice { get; internal set; }

    public TimeSpan FixedStepTarget = TimeSpan.FromSeconds(1.0f/60.0f);
    public TimeSpan FixedStepMaxElapsed = TimeSpan.FromSeconds(5.0f/60.0f);

    private TimeSpan lastTime;
    private TimeSpan accumulator;
    private Stopwatch timer = new Stopwatch();

    public bool Exiting { get; private set;}

    internal static GameApp Instance;

    public GameApp(WindowSettings settings, GraphicsSettings graphicsSettings)
    {
        Instance = this;
        BackendFlags backendFlags = 0;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) 
        {
            backendFlags = BackendFlags.D3D11;
        }
        else if (Environment.OSVersion.Platform == PlatformID.Unix) 
        {
            backendFlags = BackendFlags.Vulkan;
        }
        else 
        {
            backendFlags = BackendFlags.Metal;
        }

        SDL.SDL_WindowFlags windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;

        if (backendFlags != BackendFlags.D3D11) 
        {
            windowFlags |= backendFlags switch 
            {
                BackendFlags.Vulkan => SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN,
                BackendFlags.Metal => SDL.SDL_WindowFlags.SDL_WINDOW_METAL,
                _ => throw new Exception("Not Supported")
            };
        }

        MainWindow = new Window(settings, windowFlags);
        GraphicsDevice = new GraphicsDevice(graphicsSettings, backendFlags);

        if (!GraphicsDevice.ClaimWindow(MainWindow, graphicsSettings.SwapchainComposition, graphicsSettings.PresentMode))
        {
            throw new Exception("Cannot claim this window.");
        }

        Logger.Info("Successfully claimed a window.");

        AudioDevice = new AudioDevice();
        Audio.Init(AudioDevice);
        InputDevice = new InputDevice();
        Input.Init(InputDevice);

        Width = (int)settings.Width;
        Height = (int)settings.Height;

        GameContext.Init(GraphicsDevice, MainWindow);
        Assets = new AssetStorage(AudioDevice, AssetPath);
        ResourceUploader uploader = new ResourceUploader(GraphicsDevice);
        Assets.StartContext(uploader);
        LoadContent(Assets);
        Assets.EndContext();
        Scene = Initialize();
    }

    /// <summary>
    /// A method for loading content. 
    /// </summary>
    public virtual void LoadContent(AssetStorage storage) {}
    /// <summary>
    /// Called when the application exited.
    /// </summary>
    public virtual void Destroy() {}


    /// <summary>
    /// A method to also initialize your other resources, and to set your scene.
    /// </summary>
    /// <returns>A <see cref="Riateu.Scene"/> or <see cref="Riateu.GameLoop"/></returns>
    public abstract GameLoop Initialize();

    private void InternalUpdate(float delta) 
    {
        if (scene == null || (scene != nextScene))
        {
            scene?.End();
            scene = nextScene;
            scene.Begin();
        }

        scene.Update(Time.Delta);
    }
    private void InternalRender() 
    {
        CommandBuffer cmdBuf = GraphicsDevice.AcquireCommandBuffer();
        GraphicsDevice.DeviceClaimCommandBuffer(cmdBuf);
        RenderTarget backbuffer = cmdBuf.AcquireSwapchainTarget(MainWindow);
        if (backbuffer != null) 
        {
            scene.Render(backbuffer);
        }

        Time.Draw();
        GraphicsDevice.Submit(cmdBuf);
    }

    public void Run() 
    {
        Logger.Info("Showing the Main Window");
        MainWindow.Show();
        
        Logger.Info("Recalibrating Input Device.");

        InputDevice.Update();
        timer.Restart();

        Logger.Info("Game Started...");
        while (!Exiting) 
        {
            Tick();
        }

        Destroy();
        GraphicsDevice.UnclaimWindow(MainWindow);

        Logger.Info("Closing Window.");
        MainWindow.Dispose();
        Logger.Info("Destroying Audio Thread/Device.");
        AudioDevice.Dispose();
        Logger.Info("Destroying Graphics Device.");
        GraphicsDevice.Dispose();

        SDL.SDL_Quit();
        Logger.Info("Game Exited...");
    }

    public void Tick() 
    {
        TimeSpan currentTime = timer.Elapsed;
        TimeSpan deltaTime = currentTime - lastTime;
        lastTime = currentTime;

        accumulator += deltaTime;

        while (accumulator < FixedStepTarget) 
        {
            int ms = (int)(FixedStepTarget - accumulator).TotalMilliseconds;

            Thread.Sleep(ms);

            currentTime = timer.Elapsed;
            deltaTime = currentTime - lastTime;
            lastTime = currentTime;
            accumulator += deltaTime;
        }

        PollEvents();

        if (accumulator > FixedStepMaxElapsed) 
        {
            AdvanceDeltaTime(accumulator - FixedStepMaxElapsed);
            accumulator = FixedStepMaxElapsed;
        }

        while (accumulator >= FixedStepTarget) 
        {
            accumulator -= FixedStepTarget;
            Time.Frame++;
            AdvanceDeltaTime(FixedStepTarget);
            InputDevice.Update();
            InternalUpdate(Time.Delta);
            AudioDevice.Reset();
            if (Exiting) 
            {
                return;
            }
        }

        InternalRender();
    }

    public void Quit() 
    {
        Exiting = true;
    }

    private void AdvanceDeltaTime(TimeSpan delta) 
    {
        Time.RawDelta = (float)delta.TotalSeconds;
        Time.Delta = Time.RawDelta * Time.DeltaScale;
        Time.Duration += delta;
    }

    private void PollEvents() 
    {
        while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1) 
        {
            switch (e.type) 
            {
            case SDL.SDL_EventType.SDL_QUIT:
                Exiting = true;
                break;
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                InputDevice.Mouse.WheelRawX += e.wheel.x;
                InputDevice.Mouse.WheelRawY += e.wheel.y;
                break;
            case SDL.SDL_EventType.SDL_WINDOWEVENT:
                if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED) 
                {
                    MainWindow.HandleSizeChanged((uint)e.window.data1, (uint)e.window.data2);
                }
                else if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE) 
                {
                    GraphicsDevice.UnclaimWindow(MainWindow);
                    MainWindow.Dispose();
                }
                break;
            case SDL.SDL_EventType.SDL_TEXTINPUT:
                HandleTextInput(e);
                break;
            }
        }
    }

    private unsafe void HandleTextInput(SDL.SDL_Event evt) 
    {
        byte *textPtr = evt.text.text;
        int count = 0;
        while (*textPtr != 0) 
        {
            textPtr++;
            count++;
        }

        if (count > 0) 
        {
            char *charPtr = stackalloc char[count];
            int chars = Encoding.UTF8.GetChars(evt.text.text, count, charPtr, count);

            for (int i = 0; i < chars; i++) 
            {
                Input.Keyboard.WriteCharacter(charPtr[i]);
            }
        }
    }
}

public record struct GraphicsSettings(SwapchainComposition SwapchainComposition, PresentMode PresentMode, bool DebugMode = false, bool LowPowerMode = false) 
{
    public static GraphicsSettings Default = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Immediate, false, false);
    public static GraphicsSettings Debug = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Immediate, true, false);
    public static GraphicsSettings Vsync = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.VSync, false, false);
    public static GraphicsSettings DebugVSync = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.VSync, true, false);
    public static GraphicsSettings Fast = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Mailbox, false, false);
    public static GraphicsSettings DebugFast = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Mailbox, true, false);
}
