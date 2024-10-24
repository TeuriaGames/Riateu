using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Riateu.Audios;
using Riateu.Graphics;

namespace Riateu.Content;

public interface IAssets {}

public class Ref<T>
{
    public T Data 
    {
        get => data;
        set 
        {
            data = value;
        }
    }
    private T data;

    public Ref(T data) 
    {
        this.data = data;
    }

    public static implicit operator T(Ref<T> refs) 
    {
        return refs.data;
    }
}

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
        watcher.Dispose();
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

public class AssetStorage
{
    private string assetPath;
    private ResourceUploader uploader;
    private Packer<AtlasItem> packer;
    private AudioDevice audioDevice;
    private GraphicsDevice graphicsDevice;
#if DEBUG
    private AssetServer server;
#endif

    internal AssetStorage(GraphicsDevice graphicsDevice, AudioDevice audioDevice, string path) 
    {
        assetPath = path;
        packer = new Packer<AtlasItem>(8192);
        this.audioDevice = audioDevice;
        this.graphicsDevice = graphicsDevice;
    }

#if DEBUG
    internal AssetStorage(GraphicsDevice graphicsDevice, AudioDevice audioDevice, AssetServer server, string path) 
        : this(graphicsDevice, audioDevice, path)
    {
        this.server = server;
    }
#endif

    public void StartContext() 
    {
        uploader = new ResourceUploader(graphicsDevice);
        server.Reset();
    }

    /// <summary>
    /// Create an <see cref="Riateu.Graphics.Atlas"/> from a folder. This will look all of PNGs file recursively in this folder.
    /// </summary>
    /// <param name="basePath">A target folder to look for</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public Ref<Atlas> CreateAtlas(string basePath) 
    {
        void Crawl(string path) 
        {
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories) 
            {
                Crawl(directory);
            }

            var files = Directory.GetFiles(path).Where(x => x.EndsWith("png") || x.EndsWith("gif"));
            foreach (var file in files) 
            {
                string name = Path.Join(path, Path.GetFileNameWithoutExtension(file)).Replace('\\', '/')
                    .Substring(basePath.Length + 1);

                if (Path.GetExtension(file) == ".gif") 
                {
                    Span<Image> images = Image.LoadGif(file);
                    for (int i = 0; i < images.Length; i++) 
                    {
                        Image image = images[i];
                        packer.Add(new Packer<AtlasItem>.Item(
                            new AtlasItem($"{name}/{i}", image), image.Width, image.Height));
                    }
                }
                else 
                {
                    Image image = new Image(file);
                    packer.Add(new Packer<AtlasItem>.Item(new AtlasItem(name, image), image.Width, image.Height));
                }

            }
        }
        if (basePath.EndsWith(Path.DirectorySeparatorChar)) 
        {
            basePath = basePath.Substring(0, basePath.Length - 1);
        }

        Crawl(basePath);

        if (packer.Pack(out List<Packer<AtlasItem>.PackedItem> items, out Point size)) 
        {
            Ref<Atlas> atlas = new Ref<Atlas>(Atlas.LoadFromPacker(uploader, items, size));
#if DEBUG
            server.TrackResource(atlas.Data.BaseTexture);
            server.AddReactivePath(basePath, AssetServer.ReactiveType.Folder);
#endif
            return atlas;
        }

        return null;
    }

    /// <summary>
    /// Create an <see cref="Riateu.Graphics.Atlas"/> from a packer.
    /// </summary>
    /// <param name="packer">A packer that contains all of the added images</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public Ref<Atlas> CreateAtlasFromPacker(Packer<AtlasItem> packer) 
    {
        if (packer.Pack(out List<Packer<AtlasItem>.PackedItem> items, out Point size)) 
        {
            return new Ref<Atlas>(Atlas.LoadFromPacker(uploader, items, size));
        }
        return null;
    }

    public Shader LoadSPIRVVertexShader(string path, uint uniformBufferCount = 0) 
    {
        return new Shader(graphicsDevice, path, "main", new ShaderCreateInfo() 
        {
            ShaderFormat = ShaderFormat.SPIRV,
            ShaderStage = ShaderStage.Vertex,
            UniformBufferCount = uniformBufferCount
        });
    }

    public Shader LoadSPIRVFragmentShader(string path, uint samplerCount, uint uniformBufferCount = 0) 
    {
        return new Shader(graphicsDevice, path, "main", new ShaderCreateInfo() 
        {
            ShaderFormat = ShaderFormat.SPIRV,
            ShaderStage = ShaderStage.Fragment,
            SamplerCount = samplerCount,
            UniformBufferCount = uniformBufferCount
        });
    }

    public Ref<Texture> LoadTexture(string path) 
    {
        Ref<Texture> texture = new Ref<Texture>(uploader.CreateTexture2DFromCompressed(path));
#if DEBUG
        server.TrackResource(texture.Data);
        server.AddReactivePath(path, AssetServer.ReactiveType.File);
#endif
        return texture;
    }

    public AudioStream LoadAudioStream(string path, AudioFormat format) 
    {
        AudioStream stream = format switch 
        {
            AudioFormat.OGG => new AudioStreamOGG(audioDevice, path),
            AudioFormat.WAV => throw new NotImplementedException("WAV format has not been implemented yet"),
            _ => throw new InvalidOperationException("Unknown format is passed.")
        };

#if DEBUG
        server.TrackResource(stream);
        server.AddReactivePath(path, AssetServer.ReactiveType.File);
#endif

        return stream;
    }

    public AudioTrack LoadAudioTrack(string path, AudioFormat format) 
    {
        AudioTrack track = format switch 
        {
            AudioFormat.OGG => AudioTrackOGG.CreateOGG(audioDevice, path),
            AudioFormat.WAV => AudioTrackWAV.CreateWAV(audioDevice, path),
            _  => throw new InvalidOperationException("Unknown format is passed.")
        };
#if DEBUG
        server.TrackResource(track);
        server.AddReactivePath(path, AssetServer.ReactiveType.File);
#endif
        return track;
    }

    public Ref<Atlas> LoadAtlas(string path, Texture texture, bool ninePatchEnabled = false, JsonType fileType = JsonType.Json) 
    {
        Ref<Atlas> atlas = new Ref<Atlas>(Atlas.LoadFromFile(path, texture, fileType, ninePatchEnabled));
#if DEBUG
        server.TrackResource(atlas.Data.BaseTexture);
        server.AddReactivePath(path, AssetServer.ReactiveType.File);
#endif
        return atlas;
    }

    public SpriteFont LoadFont(string path, float size) 
    {
        SpriteFont spriteFont = new SpriteFont(uploader, path, size, SpriteFont.DefaultCharset);
#if DEBUG
        server.TrackResource(spriteFont.Texture);
        server.AddReactivePath(path, AssetServer.ReactiveType.File);
#endif
        return spriteFont;
    }

    public SpriteFont LoadFont(string path, float size, ReadOnlySpan<int> charset) 
    {
        SpriteFont spriteFont = new SpriteFont(uploader, path, size, charset);
#if DEBUG
        server.TrackResource(spriteFont.Texture);
        server.AddReactivePath(path, AssetServer.ReactiveType.File);
#endif
        return spriteFont;
    }

    public AnimationStorage LoadAnimations(string path, Atlas atlas, JsonType fileType = JsonType.Json) 
    {
        AnimationStorage storage = AnimationStorage.Create(path, atlas, fileType);
#if DEBUG
        server.AddReactivePath(path, AssetServer.ReactiveType.File);
#endif
        return storage;
    }

    public void EndContext() 
    {
        uploader.Upload();
        uploader.Dispose();
    }
}