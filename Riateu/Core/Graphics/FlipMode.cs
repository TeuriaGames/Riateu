using System;

namespace Riateu.Graphics;

/// <summary>
/// An enum flags to set the flip mode of a UV texture.
/// </summary>
[Flags]
public enum FlipMode : byte
{
    /// <summary>
    /// No flip mode will be set.
    /// </summary>
    None,
    /// <summary>
    /// The UV texture will be flip horizontally.
    /// </summary>
    Horizontal,
    /// <summary>
    /// The UV texture will be flip vertically.
    /// </summary>
    Vertical
}