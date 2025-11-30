using System;
using System.Text.Json;
using System.IO;
using static System.Environment;
using System.Text.Json.Serialization.Metadata;
using System.Text;

namespace Riateu;

public static class SaveIO 
{
    /// <summary>
    /// A path to where the save file directory is located.
    /// </summary>
    public static string SavePath { get; private set; }

    internal static void Init(string windowTitle) 
    {
        SavePath = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), windowTitle);
        if (!Directory.Exists(SavePath)) 
        {
            Directory.CreateDirectory(SavePath);
        }
    }

    /// <summary>
    /// Write your saves to a file to the save file directory in json format.
    /// </summary>
    /// <param name="filename">A filename of your json</param>
    /// <param name="obj">A structured object of your save</param>
    /// <param name="info">A type info of an object</param>
    /// <typeparam name="T">A type of a save object</typeparam>
    public static void SaveJson<T>(string filename, T obj, JsonTypeInfo<T> info) 
    {
        string serializedObj = JsonSerializer.Serialize<T>(obj, info);
        Save(filename, Encoding.UTF8.GetBytes(serializedObj));
    }

    /// <summary>
    /// Write your saves to a file to the save file directory.
    /// </summary>
    /// <param name="filename">A filename</param>
    /// <param name="chunks">A universal chunks of your saves</param>
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

        using (var fs = File.Create(atomicSave))
        {
            fs.Write(chunks);
        }

        if (isAtomic) 
        {
            File.Move(atomicSave, savePath, true);
        }
    }

    /// <summary>
    /// Read the save file from your save file directory in json format.
    /// </summary>
    /// <param name="filename">A name of the json file</param>
    /// <param name="info">A type info of an object</param>
    /// <typeparam name="T">A type of a save object</typeparam>
    /// <returns>A structured save object</returns>
    public static T LoadJson<T>(string filename, JsonTypeInfo<T> info) 
    {
        byte[] save = Load(filename);
        string str = new string(Encoding.UTF8.GetChars(save));

        return JsonSerializer.Deserialize<T>(str, info);
    }

    /// <summary>
    /// Read the save file from your save file directory.
    /// </summary>
    /// <param name="filename">A filename to read</param>
    /// <returns>A universal save chunks</returns>
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