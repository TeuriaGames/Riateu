using System;
using System.Collections.Generic;
using SDL3;

namespace Riateu.Inputs;

public class InputDevice 
{
    public delegate void OnGamepadConnected(uint slot);
    public delegate void OnGamepadDisconnected(uint slot);

    public event OnGamepadConnected GamepadConnected;
    public event OnGamepadDisconnected GamepadDisconnected;

    public int MaxGamepad { get; private set; } = 4;
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

        foreach (BindableInput input in BindableInputs) 
        {
            input.UpdateInternal();
        }
    }

    internal bool IsGamepadConnected(uint index) 
    {
        if (index < 0 || index >= MaxGamepad) 
        {
            return false;
        }

        return !gamepads[index].NotConnected;
    }

    internal void AddGamepad(uint index) 
    {
        for (uint i = 0; i < MaxGamepad; i += 1) 
        {
            if (IsGamepadConnected(i))
            {
                continue;
            }

            var res = SDL.SDL_OpenGamepad(index);

            if (res == IntPtr.Zero)
            {
                Logger.Error("Error, failed opening gamepad!");
                Logger.Error(SDL.SDL_GetError());
                continue;
            }
            
            gamepads[i].Open(res);
            Logger.Info($"Gamepad {i} is connected!");
            GamepadConnected?.Invoke(i);
        }
    }

    internal void RemoveGamepad(uint index) 
    {
        for (uint i = 0; i < MaxGamepad; i+= 1)
        {
            if (index == gamepads[i].InstanceID) 
            {
                SDL.SDL_CloseGamepad(gamepads[i].Handle);
                Logger.Info($"Gamepad {i} is disconnected!");
                gamepads[i].Close();
                GamepadDisconnected?.Invoke(index);
            }
        }
    }


    internal void FindController() 
    {
    }

    public void SetGamepadCount(int count) 
    {
        Array.Resize<Gamepad>(ref gamepads, count);
    }
}
