using System;
using System.Collections.Generic;
using System.IO;

namespace Riateu.Content;

internal class AssetServer : IDisposable
{
    public enum ReactiveType { File, Folder }
    private HashSet<string> reactiveFiles = new HashSet<string>();
    private HashSet<string> reactiveFolders = new HashSet<string>();
    private List<IDisposable> trackedResources = new List<IDisposable>();
    private FileSystemWatcher watcher;
    private bool dirty;
    private Action<AssetStorage> actionReload;
    private object someLock = new object();

    public AssetServer() {}
    
    public AssetServer(string assetPath) 
    {
        if (!Directory.Exists(assetPath)) 
        {
            Logger.Warn("AssetServer failed to watch directory, couldn't found 'Assets' folder in the root directory.");
            return;
        }
        watcher = new FileSystemWatcher(assetPath);
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.Changed += OnFileChanged;
    }

    public void TrackResource(IDisposable resource) 
    {
        trackedResources.Add(resource);
    }

    public void AddReactivePath(string path, ReactiveType type) 
    {
        switch (type) 
        {
        case ReactiveType.File:
            reactiveFiles.Add(path);
            break;
        case ReactiveType.Folder:
            reactiveFolders.Add(path);
            break;
        }
    }

    public void Reset() 
    {
        reactiveFiles.Clear();
        reactiveFolders.Clear();
    }

    public void SetWatchMethod(Action<AssetStorage> storageAction) 
    {
        actionReload = storageAction;
    }

    public void Update(AssetStorage storage) 
    {
        lock (someLock) 
        {
            if (dirty) 
            {
                foreach (var res in trackedResources) 
                {
                    res.Dispose();
                }
                trackedResources.Clear();
                storage.StartContext();
                actionReload?.Invoke(storage);
                storage.EndContext();
                dirty = false;
            }
        }
    }

    public void Dispose()
    {
        watcher?.Dispose();
        Logger.Info("Asset Server finished.");
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (someLock)
        {
            if (reactiveFiles.Contains(e.FullPath))
            {
                Logger.Info($"Asset Changes: '{e.FullPath}' reloading content.");
                dirty = true;
                return;
            }

            foreach (var folder in reactiveFolders)
            {
                if (e.FullPath.Contains(folder))
                {
                    Logger.Info($"Asset Changes: '{folder}' reloading content.");
                    dirty = true;
                    return;
                }
            }
        }
    }
}
