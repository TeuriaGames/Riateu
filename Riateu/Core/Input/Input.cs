using System;
using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Input;

namespace Riateu;

/// <summary>
/// A static class for defining all inputs.
/// </summary>
public static class Input 
{
    internal static List<BindableInput> BindableInputs = new();
    /// <summary>
    /// Disable all input updates.
    /// </summary>
    public static bool Disabled;
    /// <summary>
    /// A global input system from MoonWorks.
    /// </summary>
    public static Inputs InputSystem;

    internal static void Initialize(Inputs input) 
    {
        InputSystem = input;
        InputSystem.OnGamepadConnected += GamepadConnected;
        InputSystem.OnGamepadDisconnected += GamepadDisconnected;
    }

    internal static void Update() 
    {
        if (Disabled)
            return;
        foreach (var bind in BindableInputs) 
        {
            bind.UpdateInternal();
        }
    }

    private static void GamepadConnected(int slot)
    {
        Logger.LogInfo($"[Gamepad {slot}] Connected!");
    }

    private static void GamepadDisconnected(int slot)
    {
        Logger.LogInfo($"[Gamepad {slot}] Disconnected!");
    }
}