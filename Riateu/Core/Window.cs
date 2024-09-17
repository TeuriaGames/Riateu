using System;
using System.Collections.Generic;
using Riateu.Graphics;
using SDL2;

namespace Riateu;

public class Window : IDisposable
{
    public string Title { get; private set; }
    public IntPtr Handle { get; internal set; }

    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public WindowMode WindowMode 
    {
        get => windowMode;
        set => windowMode = value;
    }
    private WindowMode windowMode;
    private uint id;

    public SwapchainComposition SwapchainComposition { get; internal set; }
    public RenderTarget SwapchainTarget { get; internal set; }
    public TextureFormat SwapchainFormat { get; internal set; }

    public bool Claimed { get; internal set; }
    public static int TotalWindowCount => windows.Count;
    private bool IsDisposed;

    private static Dictionary<uint, Window> windows = new Dictionary<uint, Window>();
    private static uint totalWindows;
    private static Stack<uint> freedWindowID = new Stack<uint>();

    public Action<uint, uint> OnSizeChange = delegate {};

    private Window(WindowSettings settings, SDL.SDL_WindowFlags flags, uint id) 
    {
        if (settings.WindowMode == WindowMode.Fullscreen) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }
        else if (settings.WindowMode == WindowMode.BorderlessFullscreen) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        }

        if (settings.Flags.Resizable) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        if (settings.Flags.StartMaximized) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
        }

        this.windowMode = settings.WindowMode;

        SDL.SDL_GetDesktopDisplayMode(0, out var mode);

        Handle = SDL.SDL_CreateWindow(
            settings.Title,
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            settings.WindowMode == WindowMode.Windowed ? (int)settings.Width : mode.w,
            settings.WindowMode == WindowMode.Windowed ? (int)settings.Height : mode.h,
            flags
        );

        SDL.SDL_GetWindowSize(Handle, out int width, out int height);

        Width = (uint)width;
        Height = (uint)height;
        this.id = id;
    }

    public static Window CreateWindow(WindowSettings settings, SDL.SDL_WindowFlags flags) 
    {
        Window window;
        if (freedWindowID.TryPop(out uint freedID)) 
        {
            window = new Window(settings, flags, freedID);
            windows.Add(window.id, window);
        }
        else 
        {
            window = new Window(settings, flags, totalWindows);
            windows.Add(window.id, window);
            totalWindows += 1;            
        }
        return window;
    }

    public void SetWindowSize(uint width, uint height) 
    {
        SDL.SDL_SetWindowSize(Handle, (int)width, (int)height);
        Width = width;
        Height = height;

        if (WindowMode == WindowMode.Windowed) 
        {
            SDL.SDL_SetWindowPosition(Handle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
        }
    }
    public void SetScreenMode(WindowMode windowMode)
    {
        SDL.SDL_WindowFlags windowFlag = 0;

        if (windowMode == WindowMode.Fullscreen)
        {
            windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }
        else if (windowMode == WindowMode.BorderlessFullscreen)
        {
            windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        }

        SDL.SDL_SetWindowFullscreen(Handle, (uint) windowFlag);

        if (windowMode == WindowMode.Windowed)
        {
            SDL.SDL_SetWindowPosition(Handle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
        }

        WindowMode = windowMode;
    }

    public void Show() 
    {
        SDL.SDL_ShowWindow(Handle);
    }

    internal void HandleSizeChanged(uint width, uint height) 
    {
        Width = width;
        Height = height;

        OnSizeChange?.Invoke(width, height);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            SDL.SDL_DestroyWindow(Handle);
            IsDisposed = true;
        }
    }

    ~Window()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public record struct WindowSettings(string Title, uint Width, uint Height, WindowMode WindowMode = WindowMode.Windowed, Flags Flags = default);

public record struct Flags(bool Resizable = false, bool StartMaximized = false);

public enum WindowMode 
{
    Windowed,
    Fullscreen,
    BorderlessFullscreen
}