using System;
using System.Collections.Generic;
using System.Diagnostics;
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

public record struct AtlasItem(string Name, Image Image);

public class AssetStorage
{
    private enum WatchType { Texture, PackerAtlas, Atlas }
    private Dictionary<string, IAssets> assetsCache = new Dictionary<string, IAssets>();
    private string assetPath;
    private ResourceUploader uploader;
    private Packer<AtlasItem> packer;
    private AudioDevice audioDevice;
    private GraphicsDevice graphicsDevice;

#if DEBUG
    private Dictionary<WatchType, List<string>> watchlist = new ();
    private Dictionary<string, Action<string>> reactChanges = new ();
    private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
#endif

    public AssetStorage(GraphicsDevice graphicsDevice, AudioDevice audioDevice, string path) 
    {
        assetPath = path;
        packer = new Packer<AtlasItem>(8192);
        this.audioDevice = audioDevice;
        this.graphicsDevice = graphicsDevice;
#if DEBUG
        watchlist.Add(WatchType.Texture, new List<string>());
        watchlist.Add(WatchType.PackerAtlas, new List<string>());
        watchlist.Add(WatchType.Atlas, new List<string>());
#endif
    }

    public void StartContext(ResourceUploader uploader) 
    {
        this.uploader = uploader;
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
            AddToWatchlist(basePath, WatchType.PackerAtlas, path => {
                packer = new Packer<AtlasItem>(8192);
                if (path.EndsWith(Path.DirectorySeparatorChar)) 
                {
                    path = path.Substring(0, path.Length - 1);
                }

                Crawl(path);

                if (packer.Pack(out List<Packer<AtlasItem>.PackedItem> items, out Point size)) 
                {
                    atlas.Data = Atlas.LoadFromPacker(uploader, items, size);
                }
            });
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
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (Ref<Texture>)asset;
        }
        AddToWatchlist(path, WatchType.Texture, path => new Ref<Texture>(uploader.CreateTexture2DFromCompressed(path)));
        return new Ref<Texture>(uploader.CreateTexture2DFromCompressed(path));
    }

    public AudioStreamOGG LoadAudioStream(string path, AudioFormat format) 
    {
        switch (format) 
        {
        case AudioFormat.OGG:
            return new AudioStreamOGG(audioDevice, path);
        case AudioFormat.WAV:
            throw new NotImplementedException("WAV format has not been implemented yet");
        default:
            throw new InvalidOperationException("Unknown format is passed.");
        }
    }

    public AudioTrack LoadAudioTrack(string path, AudioFormat format) 
    {
        switch (format) 
        {
        case AudioFormat.OGG:
            return AudioTrackOGG.CreateOGG(audioDevice, path);
        case AudioFormat.WAV:
            return AudioTrackWAV.CreateWAV(audioDevice, path);
        default:
            throw new InvalidOperationException("Unknown format is passed.");
        }
    }

    public Ref<Atlas> LoadAtlas(string path, Texture texture, bool ninePatchEnabled = false, JsonType fileType = JsonType.Json) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (Ref<Atlas>)asset;
        }
        Ref<Atlas> atlas = new Ref<Atlas>(Atlas.LoadFromFile(path, texture, fileType, ninePatchEnabled));
        AddToWatchlist(path, WatchType.Atlas, path => new Ref<Atlas>(
            Atlas.LoadFromFile(path, texture, fileType, ninePatchEnabled))
        );
        return atlas;
    }

    public SpriteFont LoadFont(string path, float size) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (SpriteFont)asset;
        }

        SpriteFont spriteFont = new SpriteFont(uploader, path, size, SpriteFont.DefaultCharset);
        assetsCache.Add(path, spriteFont);
        return spriteFont;
    }

    public SpriteFont LoadFont(string path, float size, ReadOnlySpan<int> charset) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (SpriteFont)asset;
        }

        SpriteFont spriteFont = new SpriteFont(uploader, path, size, charset);
        assetsCache.Add(path, spriteFont);
        return spriteFont;
    }

    public AnimationStorage LoadAnimations(string path, Atlas atlas, JsonType fileType = JsonType.Json) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (AnimationStorage)asset;
        }

        AnimationStorage storage = AnimationStorage.Create(path, atlas, fileType);
        assetsCache.Add(path, storage);
        return storage;
    }

    public void EndContext(bool initWatchers) 
    {
        uploader.Upload();
        uploader.Dispose();
        if (initWatchers) 
        {
            InitWatchers(WatchType.Texture);
            InitWatchers(WatchType.Atlas);
            InitWatchers(WatchType.PackerAtlas);
        }
    }

    [Conditional("DEBUG")]
    private void InitWatchers(WatchType type) 
    {
#if DEBUG
        var paths = watchlist[type];

        foreach (var path in paths) 
        {
            var watcher = new FileSystemWatcher(path);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = type == WatchType.PackerAtlas;
            if (type == WatchType.PackerAtlas) 
            {
                watcher.Changed += (s, a) => {
                    var ext = Path.GetExtension(a.FullPath);
                    if (ext is not ".png" and not ".qoi" and not ".gif") 
                    {
                        return;
                    }
                    if (reactChanges.TryGetValue(path, out var val)) 
                    {
                        StartContext(new ResourceUploader(graphicsDevice));
                        try 
                        {
                            val(path);
                        }
                        catch (Exception e) 
                        {
                            Console.WriteLine(e.ToString());
                        }
                        EndContext(false);
                    }
                };
            }
            else 
            {
                watcher.Changed += (s, a) => {
                    var parenDir = Path.GetDirectoryName(a.FullPath);
                    if (reactChanges.TryGetValue(parenDir, out var val)) 
                    {
                        StartContext(new ResourceUploader(graphicsDevice));
                        val(a.FullPath);
                        EndContext(false);
                    }
                };
            }

            watchers.Add(watcher);
        }

#endif
    }

    [Conditional("DEBUG")]
    private void AddToWatchlist(string path, WatchType type, Action<string> action) 
    {
#if DEBUG
        string dirName;
        if (type == WatchType.Atlas) 
        {
            dirName = path;
        }
        else 
        {
            dirName = Path.GetDirectoryName(path);
        }
        var list = watchlist[type];
        if (!list.Contains(dirName)) 
        {
            watchlist[type].Add(dirName);
        }
        reactChanges.Add(path, action);
#endif
    }
}