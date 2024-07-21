using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoonWorks;
using MoonWorks.Input;

namespace Riateu;

/// <summary>
/// A static class for defining all inputs.
/// </summary>
public static class Input 
{
    internal static List<BindableInput> BindableInputs = new();
    internal static Inputs InputSystem;

    /// <summary>
    /// Disable all input updates.
    /// </summary>
    public static bool Disabled;

    /// <summary>
    /// The keyboard input.
    /// </summary>
    public static Keyboard Keyboard => InputSystem.Keyboard;
    /// <summary>
    /// The mouse input.
    /// </summary>
    public static Mouse Mouse => InputSystem.Mouse;

    /// <summary>
    /// A check if any of the inputs are pressed.
    /// </summary>
    public static bool AnyPressed => InputSystem.AnyPressed;
    /// <summary>
    /// Any button that are pressed will be return. 
    /// </summary>
    public static VirtualButton AnyPressedButton => InputSystem.AnyPressedButton;

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

    /// <summary>
    /// Get the gamepad by slot.
    /// </summary>
    /// <param name="slot">An index of a gamepad</param>
    /// <returns>A gamepad input</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Gamepad GetGamepad(int slot) 
    {
        return InputSystem.GetGamepad(slot);
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