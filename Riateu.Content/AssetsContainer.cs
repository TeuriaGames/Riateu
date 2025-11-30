using System;
using System.IO;
using System.Numerics;
using ImGuiNET;

namespace Riateu.Content.App;

public class AssetsContainer 
{
    private enum ModalType { File, Folder, Delete }
    public Action<string> OnAssetSelected;
    public Action OnSelectProject;
    private ContentWindow window;

    private string inputFilename = string.Empty;
    private string currentFolderPath;
    private ModalType modalType;

    public AssetsContainer(ContentWindow window) 
    {
        this.window = window;
    }

    public void Draw() 
    {
        ImGui.Begin($"{FA6.Box} Assets");
        if (string.IsNullOrEmpty(window.SelectedProject)) 
        {
            ImGui.LabelText("##Notice", "No Project selected");
            if (ImGui.Button($"{FA6.FileImport} Select a Project")) 
            {
                OnSelectProject?.Invoke();
            }
        }
        else 
        {
            FileSystem(window.SelectedProject);
        }

        ImGui.End();
    }

    private void FileSystem(string directory) 
    {
        bool modalOpen = false;
        string[] directories = Directory.GetDirectories(directory);
        string[] files = Directory.GetFiles(directory);

        foreach (var dir in directories) 
        {
            bool open = ImGui.TreeNode($"{FA6.Folder} {Path.GetFileName(dir)}");
            if (ImGui.BeginPopupContextItem()) 
            {
                ImGui.Selectable($"{FA6.Plus} New File");
                if (ImGui.Selectable($"{FA6.Plus} New Folder")) 
                {
                    currentFolderPath = dir;
                    modalOpen = true;
                    modalType = ModalType.Folder;
                }
                if (ImGui.Selectable($"{FA6.TrashCan} Delete")) 
                {
                    currentFolderPath = dir;
                    modalOpen = true;
                    modalType = ModalType.Delete;
                }
                ImGui.EndPopup();
            }
            if (open) 
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
        if (modalOpen) 
        {
            ImGui.OpenPopup("Create New Folder");
        }

        bool alwaysOpen = true;

        ImGui.SetNextWindowSize(new Vector2(250, 100));
        if (ImGui.BeginPopupModal("Create New Folder", ref alwaysOpen, ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove)) 
        {
            switch (modalType) 
            {
            case ModalType.Folder:
                ImGui.InputText("Folder name", ref inputFilename, 100);
                if (ImGui.Button("Create")) 
                {
                    string path = Path.Combine(currentFolderPath, inputFilename);
                    if (!Directory.Exists(path)) 
                    {
                        Directory.CreateDirectory(path);
                    }
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel")) 
                {
                    ImGui.CloseCurrentPopup();
                }
                break;
            case ModalType.Delete:
                ImGui.Text("Are you sure you want to delete?");
                if (ImGui.Button("Yes")) 
                {
                    if (Directory.Exists(currentFolderPath)) 
                    {
                        Directory.Delete(currentFolderPath);
                    }
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("No")) 
                {
                    ImGui.CloseCurrentPopup();
                }
                break;
            }

            ImGui.EndPopup();
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
        return new string($"{FA6.File} {spanFile}");
    }
}