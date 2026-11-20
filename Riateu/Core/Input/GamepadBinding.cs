using System.Collections.Generic;
using Riateu.Inputs;

namespace Riateu;

public class GamepadBinding : IBinding
{
    public int Slot;
    public List<GamepadButtonCode> Buttons = new List<GamepadButtonCode>();
    public List<GamepadAxisButtonCode> AxisButtons = new List<GamepadAxisButtonCode>();

    public GamepadBinding(int slot, params GamepadButtonCode[] buttons)
    {
        Slot = slot;
        Add(buttons);
    }

    public GamepadBinding(int slot, params GamepadAxisButtonCode[] axisButtons)
    {
        Slot = slot;
        Add(axisButtons);
    }

    public void Add(params GamepadButtonCode[] buttons)
    {
        foreach (var button in buttons)
        {
            if (!Buttons.Contains(button))
            {
                Buttons.Add(button);
            }
        }
    }

    public void Add(params GamepadAxisButtonCode[] axisButtons)
    {
        foreach (var button in axisButtons)
        {
            if (!AxisButtons.Contains(button))
            {
                AxisButtons.Add(button);
            }
        }
    }

    public void Replace(params GamepadButtonCode[] buttons)
    {
        Buttons.Clear();
        AxisButtons.Clear();
        Add(buttons);
    }

    public bool JustPressed() => Any(static (b) => b.Pressed);

    public bool Pressed() => Any(static (b) => b.IsDown);

    public bool Released() => Any(static (b) => b.Released);

    private bool Any(System.Func<InputButton, bool> check)
    {
        if (!Input.Device.IsGamepadConnected(Slot))
        {
            return false;
        }
        var gamepad = Input.Gamepad(Slot);
        foreach (var button in Buttons)
        {
            if (check(gamepad.Button(button)))
            {
                return true;
            }
        }
        foreach (var button in AxisButtons)
        {
            if (check(gamepad.Button(button)))
            {
                return true;
            }
        }
        return false;
    }
}

public class GamepadAxisBinding : IAxisBinding
{
    public int Slot;
    public GamepadAxisCode Axis;
    public float Threshold;
    public GamepadButtonCode? Negative;
    public GamepadButtonCode? Positive;
    private int value;

    public GamepadAxisBinding(int slot, GamepadAxisCode axis, float threshold = 0.5f)
    {
        Slot = slot;
        Axis = axis;
        Threshold = threshold;
    }

    public GamepadAxisBinding(int slot, GamepadButtonCode negative, GamepadButtonCode positive)
    {
        Slot = slot;
        Negative = negative;
        Positive = positive;
    }

    public int GetValue() => value;

    public void Update()
    {
        value = 0;
        if (!Input.Device.IsGamepadConnected(Slot))
        {
            return;
        }
        var gamepad = Input.Gamepad(Slot);
        if (Negative is { } negative && Positive is { } positive)
        {
            value = (gamepad.IsDown(positive) ? 1 : 0) - (gamepad.IsDown(negative) ? 1 : 0);
            return;
        }
        float axis = gamepad.Axis(Axis);
        if (axis >= Threshold)
        {
            value = 1;
        }
        else if (axis <= -Threshold)
        {
            value = -1;
        }
    }
}
