using System.Collections.Generic;
using MoonWorks.Input;

namespace Riateu;

/// <summary>
/// A class that manage a keyboard binding for an input.
/// </summary>
public class KeyboardBinding : IBinding
{
    /// <summary>
    /// A collection of KeyCode that is bind to this binding.
    /// </summary>
    public List<KeyCode> Keys = new List<KeyCode>();

    private KeyboardBinding() {}
    /// <summary>
    /// An initialization of this binding.
    /// </summary>
    /// <param name="keys">An amount of key binding in this one <see cref="Riateu.KeyboardBinding"/></param>
    public KeyboardBinding(params KeyCode[] keys) 
    {
        Add(keys);
    }

    /// <summary>
    /// Add a keycode to the binding.
    /// </summary>
    /// <param name="keys">An amount of keycode binding in this one <see cref="Riateu.KeyboardBinding"/></param>
    public void Add(params KeyCode[] keys) 
    {
        foreach (var key in keys) 
        {
            if (Keys.Contains(key))
                continue;
            Keys.Add(key);
        }
    }

    /// <summary>
    /// Add a keycode to the binding.
    /// </summary>
    /// <param name="keys">An amount of id of keycode binding in this one <see cref="Riateu.KeyboardBinding"/></param>
    public void Add(params int[] keys) 
    {
        foreach (var key in keys) 
        {
            if (Keys.Contains((KeyCode)key))
                continue;
            Keys.Add((KeyCode)key);
        }
    }

    /// <summary>
    /// Replace all keycodes with these new ones.
    /// </summary>
    /// <param name="keys">A new keycodes for a replacement</param>
    public void Replace(params KeyCode[] keys) 
    {
        if (Keys.Count > 0)
            Keys.Clear();

        Add(keys);
    }

    /// <inheritdoc/>
    public bool JustPressed()
    {
        foreach (var key in Keys) 
        {
            if (Input.Keyboard.IsPressed(key)) 
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public bool Pressed()
    {
        foreach (var key in Keys) 
        {
            if (Input.Keyboard.IsDown(key)) 
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public bool Released()
    {
        foreach (var key in Keys) 
        {
            if (Input.Keyboard.IsReleased(key)) 
            {
                return true;
            }
        }
        return false;
    }
}