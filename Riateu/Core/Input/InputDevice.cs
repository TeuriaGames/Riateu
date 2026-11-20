using System;
using System.Collections.Generic;
using SDL3;

namespace Riateu.Inputs;

public class InputDevice
{
    public delegate void OnGamepadConnected(int slot);
    public delegate void OnGamepadDisconnected(int slot);

    public event OnGamepadConnected GamepadConnected;
    public event OnGamepadDisconnected GamepadDisconnected;

    public int MaxGamepad => gamepads.Length;
    public Keyboard Keyboard { get; private set; }
    public Mouse Mouse { get; private set; }
    public Gamepad[] Gamepads => gamepads;
    public bool Disabled { get; set; }

    public List<BindableInput> BindableInputs { get; private set; }

    private Gamepad[] gamepads;

    public InputDevice()
    {
        BindableInputs = new List<BindableInput>();
        Keyboard = new Keyboard();
        Mouse = new Mouse();
        gamepads = new Gamepad[4];
        for (int i = 0; i < gamepads.Length; i++)
        {
            gamepads[i] = new Gamepad(i);
        }

        Logger.Info("Input Device Created successfully!");
    }

    public void Update()
    {
        if (Disabled)
        {
            return;
        }
        Keyboard.Update();
        Mouse.Update();
        foreach (Gamepad gamepad in gamepads)
        {
            gamepad.Update();
        }

        foreach (BindableInput input in BindableInputs)
        {
            input.UpdateInternal();
        }
    }

    public Gamepad Gamepad(int slot) => gamepads[slot];

    public bool IsGamepadConnected(int slot) =>
        slot >= 0 && slot < gamepads.Length && gamepads[slot].Connected;

    internal void AddGamepad(uint instanceID)
    {
        int free = -1;
        for (int i = 0; i < gamepads.Length; i++)
        {
            if (gamepads[i].Connected && gamepads[i].InstanceID == instanceID)
            {
                return;
            }
            if (free < 0 && !gamepads[i].Connected)
            {
                free = i;
            }
        }

        if (free < 0)
        {
            Logger.Warn("No free gamepad slot available.");
            return;
        }

        IntPtr handle = SDL.SDL_OpenGamepad(instanceID);
        if (handle == IntPtr.Zero)
        {
            Logger.Error($"Failed opening gamepad: {SDL.SDL_GetError()}");
            return;
        }

        gamepads[free].Open(handle, instanceID);
        Logger.Info($"Gamepad {free} is connected: {gamepads[free].Name}");
        GamepadConnected?.Invoke(free);
    }

    internal void RemoveGamepad(uint instanceID)
    {
        for (int i = 0; i < gamepads.Length; i++)
        {
            var gamepad = gamepads[i];
            if (!gamepad.Connected || gamepad.InstanceID != instanceID)
            {
                continue;
            }
            SDL.SDL_CloseGamepad(gamepad.Handle);
            gamepad.Close();
            Logger.Info($"Gamepad {i} is disconnected!");
            GamepadDisconnected?.Invoke(i);
            return;
        }
    }

    public void SetGamepadCount(int count)
    {
        for (int i = count; i < gamepads.Length; i++)
        {
            if (gamepads[i].Connected)
            {
                SDL.SDL_CloseGamepad(gamepads[i].Handle);
                gamepads[i].Close();
                GamepadDisconnected?.Invoke(i);
            }
        }

        int old = gamepads.Length;
        Array.Resize(ref gamepads, count);
        for (int i = old; i < count; i++)
        {
            gamepads[i] = new Gamepad(i);
        }

        IntPtr ids = SDL.SDL_GetGamepads(out int connected);
        if (ids == IntPtr.Zero)
        {
            return;
        }
        unsafe
        {
            for (int i = 0; i < connected; i++)
            {
                AddGamepad(((uint*)ids)[i]);
            }
        }
        SDL.SDL_free(ids);
    }
}
