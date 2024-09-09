using System;
using System.Text.Json;
using System.IO;
using static System.Environment;
using System.Text.Json.Serialization.Metadata;
using System.Text;

namespace Riateu;

public static class SaveIO 
{
    public static string SavePath;

    public static void Init(string windowTitle) 
    {
        SavePath = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), windowTitle);
        if (!Directory.Exists(SavePath)) 
        {
            Directory.CreateDirectory(SavePath);
        }
    }

    public static void SaveJson<T>(string filename, T obj, JsonTypeInfo<T> info) 
    {
        string serializedObj = JsonSerializer.Serialize<T>(obj, info);
        Save(filename, Encoding.UTF8.GetBytes(serializedObj));
    }

    public static void Save(string filename, ReadOnlySpan<byte> chunks) 
    {
        string savePath = GetSavePath(filename);
        string atomicSave = savePath;
        bool isAtomic = false;
        if (File.Exists(savePath)) 
        {
            atomicSave = atomicSave += ".atomic";
            isAtomic = true;
        }

        using var fs = File.Create(atomicSave);
        fs.Write(chunks);

        if (isAtomic) 
        {
            File.Move(atomicSave, savePath, true);
        }
    }

    public static byte[] Load(string filename) 
    {
        string savePath = GetSavePath(filename);

        using var fs = File.OpenRead(savePath);
        byte[] b = new byte[fs.Length];
        fs.ReadExactly(b);
        return b;
    }

    private static string GetSavePath(ReadOnlySpan<char> filename) 
    {
        return Path.Join(SavePath.AsSpan(), filename);
    }
}