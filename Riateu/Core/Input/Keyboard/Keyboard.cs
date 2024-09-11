using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SDL3;

namespace Riateu.Inputs;

public class Keyboard 
{
    public bool AnyPressed { get; private set; }
    public KeyboardButton AnyPressedButton { get; private set; }


    public static Action<char> TextInput = delegate { };

    public IntPtr State { get; private set; }
    public KeyCode[] KeyCodes { get; private set; }
    public KeyboardButton[] Buttons { get; private set; }

    private static Dictionary<KeyCode, char> specialKeyCode = new Dictionary<KeyCode, char> 
    {
        {KeyCode.Home,      (char)2},
        {KeyCode.End,       (char)3},
        {KeyCode.Backspace, (char)8},
        {KeyCode.Tab,       (char)9},
        {KeyCode.Return,    (char)13},
        {KeyCode.Delete,    (char)127},
    };

    public Keyboard() 
    {
        int numKeys = 0;
        SDL.SDL_GetKeyboardState(ref numKeys);

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

        int numKeys = 0;
        State = SDL.SDL_GetKeyboardState(ref numKeys);
        foreach (KeyCode keyCode in KeyCodes) 
        {
            KeyboardButton button = Buttons[(int)keyCode];
            button.Update();

            if (button.Pressed) 
            {
                HandleSpecialInput(keyCode);
                AnyPressed = true;
                AnyPressedButton = button;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleSpecialInput(KeyCode keyCode) 
    {
        if (specialKeyCode.TryGetValue(keyCode, out char c)) 
        {
            WriteCharacter(c);
            return;
        }
        if (IsDown(KeyCode.LeftControl) && keyCode == KeyCode.V) 
        {
            WriteCharacter((char)22);
        }
    }

    internal void WriteCharacter(char c) 
    {
        TextInput?.Invoke(c);
    }

    public KeyboardButton Button(KeyCode keyCode) 
    {
        return Buttons[(int)keyCode];
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
