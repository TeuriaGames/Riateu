using System.Collections.Generic;
using Riateu.Inputs;

namespace Riateu;

/// <summary>
/// A base class for bindable inputs.
/// </summary>
public abstract class BindableInput 
{
    /// <summary>
    /// An initialization for this class, and also adds to the input system.
    /// </summary>
    public BindableInput() 
    {
        Input.Device.BindableInputs.Add(this);
    }

    internal void UpdateInternal() 
    {
        Update();
    }

    /// <summary>
    /// Update the input binds per frame.
    /// </summary>
    public abstract void Update();
    /// <summary>
    /// Deletes all binding.
    /// </summary>
    public abstract void Delete();
}

/// <summary>
/// A bindable input for keyboard and gamepad bindings.
/// </summary>
public class BindButton : BindableInput
{
    /// <summary>
    /// A collection of <see cref="Riateu.IBinding"/> that this <see cref="Riateu.BindButton"/> has.
    /// </summary>
    public List<IBinding> Bindings = new();
    /// <summary>
    /// The amount of buffer time before it would not be count as pressed.
    /// </summary>
    public float BufferTime;
    private float buffer;

    /// <summary>
    /// Check if the button binding just pressed.
    /// </summary>
    public bool JustPressed 
    {
        get 
        {
            if (Input.Disabled)
                return false;    
            
            for (int i = 0; i < Bindings.Count; i++) 
            {
                if (Bindings[i].JustPressed() || buffer > 0f)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Check if the button binding is pressed.
    /// </summary>
    public bool Pressed 
    {
        get 
        {
            if (Input.Disabled)
                return false;    
            
            for (int i = 0; i < Bindings.Count; i++) 
            {
                if (Bindings[i].Pressed())
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Check if the button binding is released.
    /// </summary>
    public bool Released
    {
        get 
        {
            if (Input.Disabled)
                return false;    
            
            for (int i = 0; i < Bindings.Count; i++) 
            {
                if (Bindings[i].Released())
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// An initialization for this class.
    /// </summary>
    /// <param name="binding">A <see cref="Riateu.IBinding"/> to add into the input</param>
    /// <param name="bufferTime">An amount of buffer time before it would not be count as pressed.</param>
    public BindButton(IBinding binding, float bufferTime) 
    {
        Bindings.Add(binding);
        BufferTime = bufferTime;
    }

    /// <summary>
    /// An initialization for this class.
    /// </summary>
    /// <param name="binding">A collection of <see cref="Riateu.IBinding"/> to add into the input</param>
    /// <param name="bufferTime">An amount of buffer time before it would not be count as pressed.</param>
    public BindButton(IBinding[] binding, float bufferTime) 
    {
        for (int i = 0; i < binding.Length; i++) 
        {
            Bindings.Add(binding[i]);
        }
        BufferTime = bufferTime;
    }

    /// <inheritdoc/>
    public override void Update()
    {
        buffer -= (float)Time.Delta;
        var pressed = false;
        for (int i = 0; i < Bindings.Count; i++) 
        {
            var binding = Bindings[i];
            if (binding.Pressed()) 
            {
                buffer = BufferTime;
            }
            else if (binding.JustPressed()) 
            {
                buffer = BufferTime;
                pressed = true;
            }
        }

        if (!pressed) 
        {
            buffer = 0;
        }
    }

    /// <inheritdoc/>
    public override void Delete()
    {
        Bindings.Clear();
    }
}