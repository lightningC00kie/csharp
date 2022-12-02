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

        string inputFile = args[0];
        string outFile = $"{inputFile}.huff";

        FileStream? fs = InputReader.OpenStream(inputFile); // get a filestream to read the input file
        if (fs == null || fs.Length == 0)
        {
            return;
        }

        var byteDict = InputReader.Read(fs); // read the file and build a dictionary of bytes with the weight of each byte
        var nodeQueue = Huffman.Nodify(byteDict); // transform dictionary into nodes and returns a priority queue containing the nodes.
        var root = Huffman.MergeNodes(nodeQueue); // merge all nodes in the queue into a tree and return the root of the tree

        // Huffman.WriteTree(root);
        FileStream outStream = new FileStream($"{outFile}", FileMode.Create);

        OutputWriter.WriteHeader(outStream);
        OutputWriter.ByteBuilder(root, outStream);
    }
}

class InputReader
{
    public static FileStream? OpenStream(string fileName)
    {
        FileStream fs;
        try
        {
            fs = new FileStream($"{fileName}", FileMode.Open);
        }
        catch (FileNotFoundException)
        {
            WriteLine("File Error");
            return null;
        }
        return fs;
    }

    public static Dictionary<byte, int> Read(FileStream fs)
    {
        int curByte;
        var byteDict = new Dictionary<byte, int>();
        while ((curByte = fs.ReadByte()) != -1)
        {
            byte b = (byte)curByte;
            if (byteDict.ContainsKey(b))
            {
                byteDict[b]++;
            }
            else
            {
                byteDict.Add(b, 1);
            }
        }
        return byteDict;
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

    public static PriorityQueue<Node, Node> Nodify(Dictionary<byte, int> byteDict)
    {
        var nodeQueue = new PriorityQueue<Node, Node>(new HuffmanQueueComparer());
        foreach (KeyValuePair<byte, int> entry in byteDict)
        {
            var node = new Node(entry.Value, null, null, entry.Key);
            nodeQueue.Enqueue(node, node);
        }
        return nodeQueue;
    }

    public static Node MergeNodes(PriorityQueue<Node, Node> nodeQueue)
    {
        while (nodeQueue.Count > 1)
        {
            var x = nodeQueue.Dequeue();
            var y = nodeQueue.Dequeue();
            var parentNode = Huffman.BuildTree(x, y);
            nodeQueue.Enqueue(parentNode, parentNode);
        }
        var root = nodeQueue.Dequeue();
        root.IsRoot = true;
        return root;
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

class OutputWriter {
    public static byte[] Header = {0x7B, 0x68, 0x75, 0x7C, 0x6D, 0x7D, 0x66, 0x66};
    public static void WriteHeader(FileStream fs) {
        fs.Write(Header, 0, Header.Length);
        fs.Position++;
    }

    public static void ByteBuilder(Node n, FileStream fs) {
        byte indicatorBit = n.IsLeaf ? (byte) 1 : (byte) 0;
        byte[] weightBytes = BitConverter.GetBytes(n.weight);        
        WriteLine(indicatorBit + weightBytes[0]);
        weightBytes[0] += indicatorBit;
        for (int i = 0; i < weightBytes.Length; i++) {
            weightBytes[i] = (byte) ((weightBytes[i] >> (i * 8)) & 0xff);
        }
        WriteBytes(weightBytes, fs);
    }   

    public static void WriteBytes(byte[] bytes, FileStream fs) {
        fs.Write(bytes, 0, bytes.Length);
        fs.Position++;
    }
}