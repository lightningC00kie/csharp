using static System.Console;

class Top
{
    public static void Main(String[] args)
    {
        if (args.Length != 2)
        {
            WriteLine("Argument Error");
            return;
        }

        string inputFile = args[0];
        string outputFile = args[1];

        var sr = InputReader.OpenStreamReader(inputFile);

        if (sr == null)
        {
            return;
        }

        Sheet sheet = new Sheet();
        string? row;
        int rowCounter = 1;
        while ((row = InputReader.ReadRow(sr)) != null)
        {
            string[] rowArray = InputReader.SplitRow(row);
            var newRow = new List<Entry>();
            Row r = new Row(rowCounter++, rowArray.Length);
            int colCounter = 1;
            foreach (String s in rowArray)
            {
                Entry e = new Entry(s, r, colCounter++);
                newRow.Add(e);
            }
            sheet.entries.Add(newRow);
        }


        // foreach (List<Entry> l in sheet.entries)
        // {
        //     foreach (Entry e in l)
        //     {
        //         // WriteLine(e.ToString());
        //         WriteLine(sheet.EvaluateCell(e));
        //     }
        // }
        Row r1 = new Row(1, 10);
        Entry e1 = new Entry("10", r1, 1);
        Entry e2 = new Entry("10", r1, 2);
        foreach (string e in Formula.ExtractOperands("=A1+B2", "+"))
        {
            WriteLine(e);
        }
        Formula f = new Formula("*", new string[] { "B1", "A2" });
        WriteLine(sheet.EvaluateFormula(f));

    }
}

class InputReader
{
    public static StreamReader? OpenStreamReader(String inputFile)
    {
        StreamReader sr;
        try
        {
            sr = new StreamReader($"./{inputFile}");
        }
        catch
        {
            WriteLine("File Error");
            return null;
        }
        return sr;
    }

    public static string? ReadRow(StreamReader sr)
    {
        var row = sr.ReadLine();
        return row == null ? row : row.TrimEnd();
    }

    public static string[] SplitRow(string row)
    {
        return row.Split(null);
    }


}

class Sheet
{
    public List<List<Entry>> entries = new List<List<Entry>>();
    public static int ResolveColName(string colName)
    {
        int counter = 0;
        for (int i = 0; i < colName.Length; i++)
        {
            counter += ((int)colName[i] - 64) * (int)(Math.Pow(26, colName.Length - i - 1));
        }
        return counter;
    }

    public bool EntryExists(string colName, string rowName)
    {
        int colNum = ResolveColName(colName);
        int rowNum = int.Parse(rowName);
        if (rowNum <= entries.Count && rowNum > 0)
        {
            if (colNum <= entries[rowNum].Count && colNum > 0)
            {
                return true;
            }
        }
        return false;
    }

    public Entry GetEntry(string name)
    {
        bool colPart = true;
        string colName = "";
        string rowName = "";
        foreach (char c in name)
        {
            if (c >= 'A' && c <= 'Z')
            {
                if (colPart)
                    colName += c;
            }
            else if (int.TryParse(c.ToString(), out _))
            {
                colPart = false;
                rowName += c;
            }
        }
        return entries[int.Parse(rowName)][ResolveColName(colName)];
    }

    public Formula ParseFormula(Entry e)
    {
        string op = Formula.ExtractOp(e.content)!;
        string[] operands = Formula.ExtractOperands(e.content, op);
        Formula f = new Formula(op, operands);

        return f;
    }

    public int EvaluateFormula(Formula formula)
    {
        int oper1 = int.Parse(formula.operands[0]);
        WriteLine(EvaluateCell(GetEntry(formula.operands[0])));
        int oper2 = int.Parse(formula.operands[1]);
        if (formula.op == "+")
        {
            return oper1 + oper2;
        }
        else if (formula.op == "-")
        {
            return oper1 - oper2;
        }
        else if (formula.op == "*")
        {
            return oper1 * oper2;
        }
        else
        {
            return oper1 / oper2;
        }
    }

    public string EvaluateCell(Entry e)
    {
        WriteLine(e);
        if (!e.IsFormula())
        {
            return e.content;
        }
        else
        {
            Formula? f = ParseFormula(e);
            
            EvaluateCell(GetEntry(f.operands[0]));
            EvaluateCell(GetEntry(f.operands[1]));
            return "1";
        }
    }
}

class Formula : Sheet
{
    public string? op { get; set; }
    public string[] operands = new string[2];

    public Formula(string op, string[] operands)
    {
        this.op = op; this.operands = operands;
    }
    public static string? ExtractOp(string formula)
    {
        string op = "";
        if (formula.Contains('+'))
        {
            op = "+";
        }
        else if (formula.Contains('-'))
        {
            op = "-";
        }
        else if (formula.Contains('*'))
        {
            op = "*";
        }
        else if (formula.Contains('/'))
        {
            op = "/";
        }
        else
        {
            return null;
        }
        return op;
    }

    public static bool CheckNameFormat(string name, out string colName, out string rowName)
    {
        colName = "";
        rowName = "";
        bool colPart = true;
        int counter = 0;
        foreach (char c in name)
        {
            if (colPart)
            {
                if ((int)c >= 65 && (int)c <= 90)
                {
                    colName += c;
                    counter++;
                }
                else
                {
                    if (counter != 0)
                    {
                        if (c < '1' || c > '9')
                        {
                            return false;
                        }
                        rowName += c;
                        colPart = false;
                        continue;
                    }
                    return false;
                }
            }
            else
            {
                if (c < '1' || c > '9')
                {
                    return false;
                }
                rowName += c;
            }
        }
        return true;
    }

    public static string[] ExtractOperands(string formula, string op)
    {
        string[] operands = formula.Substring(1).Split(op);
        return operands;
    }
}

class Entry : Sheet
{
    public string content;
    Row row;
    int col;
    override public string ToString()
    {
        return $"{this.content}, row: {this.row.rowNum}, col: {this.col}, row length: {this.row.rowLen}";
    }
    public Entry(string content, Row row, int col)
    {
        this.content = content; this.row = row; this.col = col;
    }

    public bool IsFormula()
    {
        return this.content[0] == '=' && this.content.Length > 1;
    }
}

class Row
{
    public int rowLen;
    public int rowNum;
    public Row(int rowNum, int rowLen)
    {
        this.rowNum = rowNum; this.rowLen = rowLen;
    }
}