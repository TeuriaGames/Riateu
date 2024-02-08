using MoonWorks.Input;

namespace Riateu;

/// <summary>
/// A class that manage a keyboard axis binding for an input value.
/// </summary>
public class KeyboardAxisBinding : IAxisBinding
{
    /// <summary>
    /// A behavior if two bindings are pressed.
    /// </summary>
    public enum KeyboardOverlap 
    { 
        /// <summary>
        /// Cancel means that it acts like nothing happens. (0 value will be returned).
        /// </summary>
        Cancel, 
        /// <summary>
        /// The last binding pressed will their value be returned.
        /// </summary>
        Newer, 
        /// <summary>
        /// The first binding pressed will their value be returned.
        /// </summary>
        Older 
    }
    /// <summary>
    /// A keycode for negative axis value.
    /// </summary>
    public KeyCode Negative;
    /// <summary>
    /// A keycode for positive axis value.
    /// </summary>
    public KeyCode Positive;
    /// <summary>
    /// A behavior if two bindings are pressed.
    /// </summary>
    public KeyboardOverlap WhenOverlap;
    private int value;
    private bool isTurned;

    /// <summary>
    /// An initialization for this axis binding.
    /// </summary>
    /// <param name="negative">A keycode for negative axis value</param>
    /// <param name="positive">A keycode for positive axis value</param>
    /// <param name="overlap">A behavior if two bindings are pressed</param>
    public KeyboardAxisBinding(KeyCode negative, KeyCode positive, KeyboardOverlap overlap) 
    {
        Negative = negative;
        Positive = positive;
        WhenOverlap = overlap;
    }

    /// <summary>
    /// An initialization for this axis binding.
    /// </summary>
    /// <param name="negative">A keycode id for negative axis value</param>
    /// <param name="positive">A keycode id for positive axis value</param>
    /// <param name="overlap">A behavior if two bindings are pressed</param>
    public KeyboardAxisBinding(int negative, int positive, KeyboardOverlap overlap) 
    {
        Negative = (KeyCode)negative;
        Positive = (KeyCode)positive;
        WhenOverlap = overlap;
    }

    /// <inheritdoc/>
    public int GetValue()
    {
        return value;
    }

    /// <inheritdoc/>
    public void Update() {
        bool negative = Input.InputSystem.Keyboard.IsDown(Negative);
        bool positive = Input.InputSystem.Keyboard.IsDown(Positive);
        

        if (negative && positive) 
        {
            switch (WhenOverlap) 
            {
                case KeyboardOverlap.Cancel:
                    value = 0;
                    return;
                case KeyboardOverlap.Newer when !isTurned:
                    value *= -1;
                    isTurned = true;
                    return;
                case KeyboardOverlap.Older:
                    return;
            }
        }
        if (positive) 
        {
            isTurned = false;
            value = 1;
            return;
        }
        if (negative) 
        {
            isTurned = false;
            value = -1;
            return;
        }
        isTurned = false;
        value = 0;
    }
}