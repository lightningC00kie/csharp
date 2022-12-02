using System;
using static System.Console;
using System.Collections.Generic;
using System.IO;

class Top
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            WriteLine("Argument Error");
            return;
        }

        string INPUT_FILE = args[0];

        FileStream fs;
        try
        {
            fs = new FileStream($"{INPUT_FILE}", FileMode.Open);
        }
        catch (FileNotFoundException)
        {
            WriteLine("File Error");
            return;
        }

        var nodeQueue = new PriorityQueue<Node, Node>(new HuffmanQueueComparer());
        var byteDict = new Dictionary<byte, int>();
        int curByte;

        while ((curByte = fs.ReadByte()) != -1)
        {
            byte b = (byte) curByte;
            if (byteDict.ContainsKey(b))
            {
                byteDict[b]++;
            }
            else
            {
                byteDict.Add(b, 1);
            }
        }

        if (byteDict.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<byte, int> entry in byteDict)
        {
            var node = new Node(entry.Value, null, null, entry.Key);
            nodeQueue.Enqueue(node, node);
        }

        while (nodeQueue.Count > 1)
        {
            var x = nodeQueue.Dequeue();
            var y = nodeQueue.Dequeue();
            var parentNode = Huffman.BuildTree(x, y);
            nodeQueue.Enqueue(parentNode, parentNode);
        }
        var root = nodeQueue.Dequeue();
        root.IsRoot = true;
        Huffman.WriteTree(root);
    }
}

class Huffman
{
    public static Node BuildTree(Node x, Node y)
    {
        var node = new Node(x.weight + y.weight, left: x, right: y);
        return node;
    }

    public static void WriteTree(Node root)
    {
        if (root.right == null || root.left == null)
        {
            WriteNode(root);
            return;
        }

        WriteNode(root);
        WriteTree(root.left);
        WriteTree(root.right);
    }

    public static void WriteNode(Node root)
    {
        if (!root.IsLeaf)
        {
            if (!root.IsRoot)
            {
                Write($" {root.weight}");
            }
            else
            {
                Write($"{root.weight}");
            }
        }
        else
        {
            Write($" *{root.value}:{root.weight}");
        }
    }
}

class Node
{
    public int? value { get; set; }
    public int weight { get; set; }
    public Node? left;
    public Node? right;
    public bool IsLeaf;
    public bool IsRoot;
    private static int _time = 0;
    public int TimeOfCreation;
    public Node(int weight, Node? left, Node? right, int? value = null, bool isRoot = false)
    {
        this.value = value; this.weight = weight; this.left = left; this.right = right;
        this.IsLeaf = this.value != null ? true : false;
        this.TimeOfCreation = _time++;
    }
}

class HuffmanQueueComparer : IComparer<Node>
{
    public int Compare(Node? x, Node? y)
    {
        int higherPriority = -1;
        if (x == null || y == null)
        {
            throw new ArgumentNullException();
        }

        if (x.weight < y.weight)
        {
            return higherPriority;
        }
        else if (x.weight > y.weight)
        {
            return -higherPriority;
        }
        else
        {
            if (x.IsLeaf && !y.IsLeaf)
            {
                return higherPriority;
            }
            else if (!x.IsLeaf && y.IsLeaf)
            {
                return -higherPriority;
            }

            if (x.IsLeaf && y.IsLeaf)
            {
                if (x.value < y.value)
                {
                    return higherPriority;
                }
                else
                {
                    return -higherPriority;
                }
            }

            else
            {
                if (x.TimeOfCreation < y.TimeOfCreation)
                {
                    return higherPriority;
                }
                else
                {
                    return -higherPriority;
                }
            }
        }

    }
}