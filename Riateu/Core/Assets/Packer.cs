using System;
using System.Collections.Generic;
using Riateu.Graphics;

namespace Riateu.Content;

public class Packer<T> 
{
    public struct Node(int x, int y, int w, int h) 
    {
        public int X = x;
        public int Y = y;
        public int W = w;
        public int H = h;
        public bool IsSplit;
        public int[] Splits = new int[2];
    }

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

    public struct PackedItem(Rectangle rect, T data)
    {
        public Rectangle Rect = rect;
        public T Data = data;
    }

    private List<Item> items = new List<Item>();
    private List<Node> nodes = new List<Node>();
    private int nodeCount = 0;
    private int currentRootIndex = 0;
    private Node root;

    public int MaxSize { get; set; }
    public bool UsePowerOfTwo { get; set; }

    public Packer(int maxSize = 2048, bool usePowerOfTwo = true) 
    {
        MaxSize = maxSize;
        UsePowerOfTwo = usePowerOfTwo;
    }

    public void Add(Item item) 
    {
        items.Add(item);
    }

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
                    Console.WriteLine($"Max Size exceeded: {MaxSize}. Failing out everything you pack.");
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