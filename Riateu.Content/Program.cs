using MoonWorks;
using Riateu;

public class Program : GameApp
{
    public Program(string title, uint width, uint height, ScreenMode screenMode, bool debugMode = false) :
        base(title, width, height, screenMode, debugMode)
    {
    }

    public override void Initialize()
    {
        Scene = new ContentWindow(this);
    }
    
    public static void Main(string[] args) 
    {
        Program program = new Program("Content Manager", 1024, 640, ScreenMode.Windowed);
        program.Run();
    }
}
