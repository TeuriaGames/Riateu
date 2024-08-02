using Rect = Riateu.Graphics.Rectangle;
using System.IO;
using Riateu.Content;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Diagnostics.CodeAnalysis;

namespace Riateu.Graphics;

[Experimental("RIE001")]
public class Aseprite : IAssets
{
    public enum Format : uint
    {
        RGBA = 32,
        Grayscale = 16,
        Indexed = 8
    }

    public enum ChunkType : ushort
    {
        OldPaletteChunk = 0x0004,
        OldPaletteChunk2 = 0x0011,
        LayerChunk = 0x2004,
        CelChunk = 0x2005,
        CelExtraChunk = 0x2006,
        ColorProfileChunk = 0x2007,
        ExternalFilesChunk = 0x2008,
        MaskChunk = 0x2016,
        PathChunk = 0x2017,
        TagsChunk = 0x2018,
        PaletteChunk = 0x2019,
        UserDataChunk = 0x2020,
        SliceChunk = 0x2022,
        TilesetChunk = 0x2023
    }

    [Flags]
    public enum LayerFlags 
    {
        Visible = 1,
        Editable = 2,
        LockMovement = 4,
        Background = 8,
        PreferLinkedCells = 16,
        DisplayedCollapsed = 32,
        ReferenceLayer = 64
    }

    public enum LayerType 
    {
        Normal = 0,
        Group = 1,
        Tilemap = 2
    }

    public enum BlendMode : ushort
    {
        Normal = 0,
        Multiply = 1,
        Screen = 2,
        Overlay = 3,
        Darken = 4,
        Lighten = 5,
        ColorDodge = 6,
        ColorBurn = 7,
        HardLight = 8,
        SoftLight = 9,
        Difference = 10,
        Exclusion = 11,
        Hue = 12,
        Saturation = 13,
        Color = 14,
        Luminiosity = 15,
        Addition = 16,
        Subtract = 17,
        Divide = 18
    }

    public enum CelType : ushort 
    {
        RawImageData = 0,
        LinkedCel = 1,
        CompressedImage = 2,
        CompressedTilemap = 3
    }

    public class Layer 
    {
        public LayerFlags Flags;
        public LayerType Type;
        public ushort ChildLevel;
        public BlendMode BlendMode;
        public byte Opacity;
        public string LayerName;
        public uint? TilesetIndex;
    }

    public class Cel 
    {
        public Layer Layer;
        public Point CelPos;
        public byte Opacity;
        public CelType CelType;
        public short ZIndex;
        public Image Pixels;
    }

    public class Frame 
    {
        public List<Cel> Cels = new List<Cel>();
    }


    private List<Layer> layers = new List<Layer>();
    public Frame[] Frames;

    public Aseprite(string loadPath) 
    {
        using FileStream stream = File.OpenRead(loadPath);
        Load(stream);
    }

    public Aseprite(Stream stream) 
    {
        Load(stream);
    }

    private void Load(Stream stream) 
    {
        BinaryReader reader = new BinaryReader(stream);

        byte   BYTE()       => reader.ReadByte();
        ushort WORD()       => reader.ReadUInt16();
        short  SHORT()      => reader.ReadInt16();
        uint   DWORD()      => reader.ReadUInt32();
        int    LONG()       => reader.ReadInt32();
        int    FIXED()      => reader.ReadInt32();
        float  FLOAT()      => reader.ReadSingle();
        double DOUBLE()     => reader.ReadDouble();
        ulong  QWORD()      => reader.ReadUInt64();
        long   LONG64()     => reader.ReadInt64();
        byte[] BYTES(int n) => reader.ReadBytes(n);
        string STRING()     => reader.ReadString();
        Point  POINT()      => new Point(LONG(), LONG());
        Point  SIZE()       => new Point(LONG(), LONG());
        Rect   RECT()       => new Rectangle(POINT(), SIZE());
        void   SKIP(int n)  => BYTES(n);


        uint fileSize = DWORD();
        ushort magic = WORD();
        if (magic != 0xA5E0) 
        {
            throw new System.Exception("Invalid magic number.");
        }

        uint frames = WORD();
        uint width = WORD();
        uint height = WORD();
        Format colorDepth = (Format)WORD();
        DWORD(); // flags
        WORD(); // Speed
        DWORD(); // 0
        DWORD(); // 0
        BYTE(); // palette entry
        SKIP(3); 
        WORD(); // num of colors
        BYTE(); // pixel width
        BYTE(); // pixel height
        SHORT(); // X position of grid
        SHORT(); // Y position of grid
        WORD(); // grid width
        WORD(); // grid height
        SKIP(84);

        this.Frames = new Frame[(int)frames];

        byte[] pixelBuffer = new byte[width * height * ((int)colorDepth / 8)];

        for (int i = 0; i < frames; i++) 
        {
            DWORD(); // bytes in this frame
            ushort frameMagic = WORD();
            if (frameMagic != 0xF1FA) 
            {
                throw new System.Exception("Invalid frame magic number.");
            }
            int oldChunk = WORD(); 
            int duration = WORD();
            SKIP(2);
            int newChunk = (int)DWORD();
            if (newChunk == 0) 
            {
                newChunk = oldChunk;
            }

            for (int chunk = 0; chunk < newChunk; chunk++) 
            {
                long pos = reader.BaseStream.Position;
                uint chunkSize = DWORD(); 
                ChunkType chunkType = (ChunkType)WORD(); // type
                long chunkEndData = pos + chunkSize;

                switch (chunkType) 
                {
                case ChunkType.LayerChunk: {
                    Layer layer = new Layer();
                    layer.Flags = (LayerFlags)WORD();
                    layer.Type = (LayerType)WORD();
                    layer.ChildLevel = WORD();
                    WORD();
                    WORD();
                    layer.BlendMode = (BlendMode)WORD();
                    layer.Opacity = BYTE();
                    SKIP(3);
                    layer.LayerName = STRING();
                    if (layer.Type == LayerType.Tilemap) 
                    {
                        layer.TilesetIndex = DWORD();
                    }

                    layers.Add(layer);

                    break;
                }
                case ChunkType.CelChunk: {
                    ushort layerIndex = WORD();
                    Cel cel = new Cel();
                    cel.Layer = layers[layerIndex];
                    cel.CelPos = POINT();
                    cel.Opacity = BYTE();
                    CelType celType = cel.CelType = (CelType)WORD();
                    cel.ZIndex = SHORT();

                    SKIP(5);

                    if (celType == CelType.LinkedCel) 
                    {
                        ushort frameIndex = WORD();
                        List<Cel> cels = Frames[frameIndex].Cels;
                        foreach (var tcel in cels) 
                        {
                            cel.Pixels = tcel.Pixels;
                        }
                        SKIP((int)chunkEndData);
                        continue;
                    }
                    else if (celType == CelType.CompressedTilemap) 
                    {
                        SKIP((int)chunkEndData);
                        continue;
                    }

                    ushort imgWidth = WORD();
                    ushort imgHeight = WORD();
                    Color[] pixels = new Color[imgWidth * imgHeight];
                    int decompressedSize = imgWidth * imgHeight * ((int)colorDepth / 8);

                    if (pixelBuffer.Length < decompressedSize) 
                    {
                        Array.Resize(ref pixelBuffer, decompressedSize);
                    }

                    switch (celType) 
                    {
                    case CelType.RawImageData:
                        reader.Read(pixelBuffer, 0, decompressedSize);
                        break;
                    case CelType.CompressedImage:
                        using (ZLibStream zlib = new ZLibStream(reader.BaseStream, CompressionMode.Decompress, true)) 
                        {
                            zlib.ReadExactly(pixelBuffer, 0, decompressedSize);
                        }
                        break;
                    }

                    switch (colorDepth) 
                    {
                    case Format.RGBA:
                        for (int p = 0, pb = 0; p < pixels.Length; p++, pb += 4) 
                        {
                            pixels[p] = new Color(pixelBuffer[pb], pixelBuffer[pb + 1], pixelBuffer[pb + 2], pixelBuffer[pb + 3]);
                        }
                        break;
                    case Format.Grayscale:
                        for (int p = 0, pb = 0; p < pixels.Length; p++, pb += 2) 
                        {
                            pixels[p] = new Color(pixelBuffer[pb], pixelBuffer[pb], pixelBuffer[pb], pixelBuffer[pb + 1]);
                        }
                        break;
                    case Format.Indexed:
                        break;    
                    }
                    cel.Pixels = new Image(pixels, imgWidth, imgHeight);
                    break;
                }
                case ChunkType.PaletteChunk: {
                    break;
                }
                }

                SKIP((int)chunkEndData);
            }
        }
    }
}