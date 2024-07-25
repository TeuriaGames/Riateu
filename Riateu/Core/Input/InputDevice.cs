using System.Collections.Generic;

namespace Riateu.Inputs;

public class InputDevice 
{
    public Keyboard Keyboard { get; private set; }
    public Mouse Mouse { get; private set; }
    public bool Disabled { get; set; }

    public List<BindableInput> BindableInputs { get; private set; }

    public InputDevice() 
    {
        BindableInputs = new List<BindableInput>();
        Keyboard = new Keyboard();
        Mouse = new Mouse();
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
}
