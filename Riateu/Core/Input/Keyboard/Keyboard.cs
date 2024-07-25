using System;
using SDL2;

namespace Riateu.Inputs;

public class Keyboard 
{
    public bool AnyPressed { get; private set; }
    public KeyboardButton AnyPressedButton { get; private set; }

    public IntPtr State { get; private set; }
    public KeyCode[] KeyCodes { get; private set; }
    public KeyboardButton[] Buttons { get; private set; }
    public Keyboard() 
    {
        SDL.SDL_GetKeyboardState(out int numKeys);

        KeyCodes = Enum.GetValues<KeyCode>();
        Buttons = new KeyboardButton[numKeys];
        foreach (KeyCode keyCode in KeyCodes) 
        {
            Buttons[(int)keyCode] = new KeyboardButton(this, keyCode);
        }
    }

    public void Update() 
    {
        AnyPressed = false;

        State = SDL.SDL_GetKeyboardState(out _);
        foreach (KeyCode keyCode in KeyCodes) 
        {
            KeyboardButton button = Buttons[(int)keyCode];
            button.Update();

            if (button.Pressed) 
            {
                AnyPressed = true;
                AnyPressedButton = button;
            }
        }
    }

    public bool IsPressed(KeyCode keyCode) 
    {
        return Buttons[(int)keyCode].Pressed;
    }

    public bool IsHeld(KeyCode keyCode) 
    {
        return Buttons[(int)keyCode].Held;
    }

    public bool IsDown(KeyCode keyCode) 
    {
        return Buttons[(int)keyCode].IsDown;
    }

    public bool IsReleased(KeyCode keyCode) 
    {
        return Buttons[(int)keyCode].Released;
    }

    public bool IsIdle(KeyCode keyCode) 
    {
        return Buttons[(int)keyCode].Idle;
    }

    public bool IsUp(KeyCode keyCode) 
    {
        return Buttons[(int)keyCode].IsUp;
    }
}
