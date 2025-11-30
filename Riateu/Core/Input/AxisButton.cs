using System.Collections.Generic;

namespace Riateu;

/// <summary>
/// A bindable input that inputs an axis of two key code when pressed
/// </summary>
public class AxisButton : BindableInput 
{
    /// <summary>
    /// A collection of <see cref="Riateu.IAxisBinding"/> that this <see cref="Riateu.AxisButton"/> has.
    /// </summary>
    public List<IAxisBinding> AxisBindings = new List<IAxisBinding>();
    /// <summary>
    /// A current axis value of this button.
    /// </summary>
    public int Value;
    /// <summary>
    /// A previous axis value of this button.
    /// </summary>
    public int PreviousValue;

    /// <summary>
    /// An initialization for this class.
    /// </summary>
    /// <param name="axisBinding">A <see cref="Riateu.IAxisBinding"/> to add into the input</param>
    public AxisButton(IAxisBinding axisBinding) 
    {
        AxisBindings.Add(axisBinding);
    }

    /// <summary>
    /// An initialization for this class.
    /// </summary>
    /// <param name="axisBindings">A collection <see cref="Riateu.IAxisBinding"/> to add into the input</param>
    public AxisButton(params IAxisBinding[] axisBindings) 
    {
        Add(axisBindings);
    }

    /// <summary>
    /// Add more <see cref="Riateu.IAxisBinding"/> into this button
    /// </summary>
    /// <param name="binding">A <see cref="Riateu.IAxisBinding"/> to add into the input</param>
    public void Add(params IAxisBinding[] binding) 
    {
        for (int i = 0; i < binding.Length; i++) 
        {
            var axis = binding[i];
            AxisBindings.Add(axis);
        }
    }

    /// <inheritdoc/>
    public override void Delete()
    {
        AxisBindings.Clear();
    }

    /// <inheritdoc/>
    public override void Update()
    {
        for (int i = 0; i < AxisBindings.Count; i++) 
        {
            AxisBindings[i].Update();
        }


        PreviousValue = Value;
        Value = 0;

        for (int i = 0; i < AxisBindings.Count; i++) 
        {
            var value = AxisBindings[i].GetValue();
            if (value != 0) 
            {
                Value = value;
                break;
            }
        }
    }
}