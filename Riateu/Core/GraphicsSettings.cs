using Riateu.Graphics;

namespace Riateu;

public record struct GraphicsSettings(SwapchainComposition SwapchainComposition, PresentMode PresentMode, bool DebugMode = false, bool LowPowerMode = false) 
{
    public static readonly GraphicsSettings Default = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Immediate, false, false);
    public static readonly GraphicsSettings Debug = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Immediate, true, false);
    public static readonly GraphicsSettings Vsync = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.VSync, false, false);
    public static readonly GraphicsSettings DebugVSync = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.VSync, true, false);
    public static readonly GraphicsSettings Fast = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Mailbox, false, false);
    public static readonly GraphicsSettings DebugFast = new GraphicsSettings(SwapchainComposition.SDR, PresentMode.Mailbox, true, false);
}
