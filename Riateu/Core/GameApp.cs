using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Riateu.Audios;
using Riateu.Content;
using Riateu.Graphics;
using Riateu.Inputs;
using SDL3;

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

    public string AssetPath = "Assets";

    public bool DisableAssetServer { get; set; }
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
#if DEBUG
    private AssetServer server;
#endif

    public bool Exiting { get; private set;}

    internal static GameApp Instance;

    public GameApp(WindowSettings settings, GraphicsSettings graphicsSettings)
    {
        Instance = this;

        if (!SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_TIMER | SDL.SDL_InitFlags.SDL_INIT_GAMEPAD | SDL.SDL_InitFlags.SDL_INIT_VIDEO)) 
        {
            Logger.Error("Failed to initialize SDL");
            return;
        }
        Logger.InitSDLLog();

        MainWindow = Window.CreateWindow(settings);
        GraphicsDevice = new GraphicsDevice(graphicsSettings);

        if (!GraphicsDevice.ClaimWindow(MainWindow, graphicsSettings.SwapchainComposition, graphicsSettings.PresentMode))
        {
            throw new Exception($"Cannot claim this window. {SDL.SDL_GetError()}");
        }

        Logger.Info("Successfully claimed a window.");
        SaveIO.Init(settings.Title.Replace(" ", "_"));

        AudioDevice = new AudioDevice();
        Audio.Init(AudioDevice);
        InputDevice = new InputDevice();
        Input.Init(InputDevice);

        Width = (int)settings.Width;
        Height = (int)settings.Height;

        GameContext.Init(GraphicsDevice, MainWindow);
#if DEBUG
        if (!DisableAssetServer) 
        {
            server = new AssetServer(AssetPath);
            server.SetWatchMethod(LoadContent);
        }
        else 
        {
            server = new AssetServer();
        }
        Assets = new AssetStorage(GraphicsDevice, AudioDevice, server, AssetPath);
#else
        Assets = new AssetStorage(GraphicsDevice, AudioDevice, AssetPath);
#endif
        Assets.StartContext();
        LoadContent(Assets);
        Assets.EndContext();

        Initialize();
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
    public abstract void Initialize();

    public abstract void Update(float delta);
    public abstract void Render();

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
#if DEBUG
        server.Dispose();
#endif
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
            Update(Time.Delta);
            AudioDevice.Reset();
            if (Exiting) 
            {
                return;
            }
        }

        Render();
        Time.UpdateCounter();
#if DEBUG
        server.Update(Assets);
#endif
    }

    public void ReloadContent()
    {
        Assets.StartContext();
        LoadContent(Assets);
        Assets.EndContext();
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
        SDL.SDL_Event e = default;
        while (SDL.SDL_PollEvent(out e)) 
        {
            switch (e.type) 
            {
            case (uint)SDL.SDL_EventType.SDL_EVENT_QUIT:
                Exiting = true;
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
                Window.CurrentFocus = Window.Windows[e.window.windowID];
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
                if (Window.CurrentFocus == Window.Windows[e.window.windowID])
                {
                    Window.CurrentFocus = null;
                }
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                InputDevice.Mouse.WheelRawX += (int)e.wheel.x;
                InputDevice.Mouse.WheelRawY += (int)e.wheel.y;
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_TEXT_INPUT:
                HandleTextInput(e);
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                Window.Windows[e.window.windowID].HandleSizeChanged((uint)e.window.data1, (uint)e.window.data2);
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                Window.Windows[e.window.windowID].Move();
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
                var index = e.gdevice.which;
                if (SDL.SDL_IsGamepad(index))
                {
                    InputDevice.AddGamepad(index);
                }
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
                InputDevice.RemoveGamepad(e.gdevice.which);
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                var window = Window.Windows[e.window.windowID];
                if (window == MainWindow) 
                {
                    Exiting = true;
                }
                GraphicsDevice.UnclaimWindow(window);
                window.Dispose();
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

        if (count <= 0)
        {
            return;
        }
        char *charPtr = stackalloc char[count];
        int chars = Encoding.UTF8.GetChars((byte*)evt.text.text, count, charPtr, count);

        for (int i = 0; i < chars; i++)
        {
            Input.Keyboard.WriteCharacter(charPtr[i]);
        }
    }
}