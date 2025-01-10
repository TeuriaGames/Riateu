using System;
using SDL3;

namespace Riateu.Inputs;

public class KeyboardButton(Keyboard keyboard, KeyCode code) : InputButton
{
    public Keyboard BaseInput = keyboard;
    public KeyCode KeyCode = code;

    internal unsafe void Update(IntPtr states) 
    {
        InputState.Update((((byte*)states)[(int)KeyCode]) != 0);
    }
}
