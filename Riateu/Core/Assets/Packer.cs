using System;
using System.Collections.Generic;
using Riateu.Graphics;

namespace Riateu.Content;

/// <summary>
/// An asset utility used to pack rectangles in very efficient manner. Useful for creating <see cref="Riateu.Graphics.Atlas"/> 
/// and <see cref="Riateu.Graphics.SpriteFont"/>.
/// </summary>
/// <typeparam name="T">A type for the item's data</typeparam>
public class Packer<T> 
{
    private struct Node(int x, int y, int w, int h) 
    {
        public int X = x;
        public int Y = y;
        public int W = w;
        public int H = h;
        public bool IsSplit;
        public int[] Splits = new int[2];
    }

    /// <summary>
    /// A struct used to contain the data, and the width and the height of a rectangle.
    /// </summary>
    public struct Item(T data, int width, int height)
    {
        public T Data = data;
        public int Width = width;
        public int Height = height;

        public uint GetTotalSize() 
        {
            int area = Width * Height;
            int largestArea = Math.Max(Width, Height);
            return (uint)(area + largestArea);
        }
    }

    /// <summary>
    /// A struct that have been outputs after packing.
    /// </summary>
    public struct PackedItem(Rectangle rect, T data)
    {
        /// <summary>
        /// A rectangle with its position offset by a packer.
        /// </summary>
        public Rectangle Rect = rect;
        /// <summary>
        /// A data of the item.
        /// </summary>
        public T Data = data;
    }

    private List<Item> items = new List<Item>();
    private List<Node> nodes = new List<Node>();
    private int nodeCount = 0;
    private int currentRootIndex = 0;
    private Node root;

    /// <summary>
    /// The maximum area size before it bails out.
    /// </summary>
    public int MaxSize { get; set; }
    /// <summary>
    /// Resize the area if it can't fit by power of two.
    /// </summary>
    public bool UsePowerOfTwo { get; set; }

    /// <summary>
    /// Construct a <see cref="Riateu.Content.Packer{T}"/>.
    /// </summary>
    /// <param name="maxSize">A maximum area size before it bails out</param>
    /// <param name="usePowerOfTwo">Sets this to true if the size has to be extends by power of two</param>
    public Packer(int maxSize = 4096, bool usePowerOfTwo = true) 
    {
        MaxSize = maxSize;
        UsePowerOfTwo = usePowerOfTwo;
    }

    /// <summary>
    /// Adds an <see cref="Riateu.Content.Packer{T}.Item"/> to pack.
    /// </summary>
    /// <param name="item">An <see cref="Riateu.Content.Packer{T}.Item"/> to add</param>
    public void Add(Item item) 
    {
        items.Add(item);
    }

    /// <summary>
    /// Pack all of the items and outputs all the results of a packed item. 
    /// This will also resets the <see cref="Riateu.Content.Packer{T}"/> state to be reused once again.
    /// </summary>
    /// <param name="packedItems">An output list of <see cref="Riateu.Content.Packer{T}.PackedItem"/></param>
    /// <param name="size">An output size</param>
    /// <returns>Returns true if succeed packing</returns>
    public bool Pack(out List<PackedItem> packedItems, out Point size) 
    {
        packedItems = new List<PackedItem>();
        if (items.Count == 0) 
        {
            size = new Point(0, 0);
            return false;
        }

        if (items.Count == 1) 
        {
            Item item = items[0];
            packedItems.Add(new PackedItem(new Rectangle(0, 0, item.Width, item.Height), item.Data));
            root = new Node(0, 0, item.Width, item.Height);
            goto DONE;
        }

        items.Sort((x, y) => {
            uint a = x.GetTotalSize();
            uint b = y.GetTotalSize();
            
            return a < b ? 1 : a > b ? -1 : 0;
        });


        root = new Node(0, 0, items[0].Width, items[0].Height);
        AddNode(root);

        for (int i = 0; i < items.Count; i++) 
        {
            Item item = items[i];

            int nodeID = FindNode(currentRootIndex, item.Width, item.Height);

            if (nodeID != -1) 
            {
                Node n = nodes[nodeID];
                SplitNode(ref n, item.Width, item.Height);
                nodes[nodeID] = n;
                Rectangle packedRect = new Rectangle(n.X, n.Y, item.Width, item.Height);
                packedItems.Add(new PackedItem(packedRect, item.Data));
            }
            else 
            {
                int growID = GrowNode(item.Width, item.Height);
                if (growID == -1) 
                {
                    Logger.Warn($"Max Size exceeded: {MaxSize}. Failing out everything you pack.");
                    // It won't fit anymore, and so we decided to break it out and fail all the attempts.
                    size = new Point(0, 0);
                    return false;
                }
                Node n = nodes[growID];
                SplitNode(ref n, item.Width, item.Height);
                nodes[growID] = n;
                Rectangle packedRect = new Rectangle(n.X, n.Y, item.Width, item.Height);
                packedItems.Add(new PackedItem(packedRect, item.Data));
            }
        }

        DONE:

        int pageWidth = 2, pageHeight = 2;

        if (UsePowerOfTwo) 
        {
            while (pageWidth < root.W)
                pageWidth *= 2;

            while (pageHeight < root.H)
                pageHeight *= 2;
        }
        else 
        {
            pageWidth = root.W;
            pageHeight = root.H;
        }

        
        size = new Point(pageWidth, pageHeight);

        // clean things up

        nodeCount = 0;
        currentRootIndex = 0;
        nodes.Clear();
        items.Clear();

        return true;
    }

    private int AddNode(Node node) 
    {
        nodes.Add(node);
        return nodeCount++;
    }

    private void SplitNode(ref Node node, int w, int h) 
    {
        node.IsSplit = true;
        node.Splits[0] = AddNode(new Node(node.X, node.Y + h, node.W, node.H - h));
        node.Splits[1] = AddNode(new Node(node.X + w, node.Y, node.W - w, node.H));
    }

    private int FindNode(int nodeID, int w, int h) 
    {
        Node node = nodes[nodeID];
        if (node.IsSplit) 
        {
            for (int i = 0; i < 2; i++) 
            {
                int splitID = node.Splits[i];
                int foundID = FindNode(splitID, w, h);
                if (foundID != -1) 
                {
                    return foundID;
                }
            }
        }
        else if ((w <= node.W) && (h <= node.H)) 
        {
            return nodeID;
        }
        return -1;
    }

    private int GrowNode(int width, int height) 
    {
        var canGrowDown = (width <= root.W) && (root.H + height < MaxSize);
        var canGrowRight = (height <= root.H) && (root.W + width < MaxSize);

        var shouldGrowRight = canGrowRight && (root.H >= (root.W + width));
        var shouldGrowDown = canGrowDown && (root.W >= (root.H + height));

        if (!canGrowDown && !canGrowRight) 
        {
            return -1;
        }

        Node oldRoot = root;
        if (shouldGrowRight || (!shouldGrowDown && canGrowRight)) 
        {
            var next = AddNode(oldRoot);
            root = new Node(0, 0, oldRoot.W + width, oldRoot.H);
            root.IsSplit = true;
            root.Splits[0] = currentRootIndex;
            currentRootIndex = next;
            nodes[next] = root;
            return root.Splits[1] = AddNode(new Node(oldRoot.W, 0, width, oldRoot.H));
        }
        else 
        {
            var next = AddNode(oldRoot);
            root = new Node(0, 0, oldRoot.W, oldRoot.H + height);
            root.IsSplit = true;
            root.Splits[1] = currentRootIndex;
            currentRootIndex = next;
            nodes[next] = root;
            return root.Splits[0] = AddNode(new Node(0, oldRoot.H, oldRoot.W, height));
        }
    }
}