using System;
using MoonWorks;

namespace Riateu.Editor;

class Program 
{
    [STAThread]
    static void Main(string[] args) 
    {
        var game = new EditorGame("Riateu", 1024, 640, ScreenMode.Windowed);
        game.Run();
    }
}