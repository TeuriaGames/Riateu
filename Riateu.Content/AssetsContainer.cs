using System;
using System.IO;
using ImGuiNET;
using NativeFileDialogSharp;

namespace Riateu.Content.App;

public class AssetsContainer 
{
    public Action<string> OnAssetSelected;
    private string pathSelected;

    public AssetsContainer(Action<string> assetSelected) 
    {
        OnAssetSelected = assetSelected;
    }

    public void Draw() 
    {
        ImGui.Begin($"{FA6.Box} Assets");
        if (pathSelected == null) 
        {
            ImGui.LabelText("##Notice", "No Project selected");
            if (ImGui.Button($"{FA6.FileImport} Select a Project")) 
            {
                DialogResult dialog = Dialog.FolderPicker("./");
                if (dialog.IsOk) 
                {
                    pathSelected = dialog.Path;
                }
            }
        }
        else 
        {
            FileSystem(pathSelected);
        }

        ImGui.End();
    }

    private void FileSystem(string directory) 
    {
        string[] directories = Directory.GetDirectories(directory);
        string[] files = Directory.GetFiles(directory);

        foreach (var dir in directories) 
        {
            if (ImGui.TreeNode(FA6.Folder + " " + Path.GetFileName(dir))) 
            {
                FileSystem(dir);
                ImGui.TreePop();
            }
        }

        foreach (var file in files) 
        {
            if (ImGui.Selectable(GetIconName(file))) 
            {
                OnAssetSelected(file);
            }
        }
    }

    private string GetIconName(ReadOnlySpan<char> file) 
    {
        var spanFile = Path.GetFileName(file);
        if (file.EndsWith("png")) 
        {
            return $"{FA6.Image} {spanFile}";
        }
        if (file.EndsWith("ttf")) 
        {
            return $"{FA6.Font} {spanFile}";
        }
        return new string(spanFile);
    }
}