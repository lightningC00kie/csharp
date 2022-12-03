using static System.Console;
using System;
using System.IO;
using System.Collections.Generic;
class Top
{
    public static void Main(string[] args)
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
            foreach (string s in rowArray)
            {
                Entry e = new Entry(s, r, colCounter++);
                newRow.Add(e);
            }
            sheet.entries.Add(newRow);
        }

        // WriteLine(sheet.EvaluateCell(sheet.GetEntry("B2")));
        Formula f = sheet.ParseFormula(sheet.GetEntry("B2"))!;
        // WriteLine(f.operands[0]);
        // WriteLine(sheet.GetEntry(f.operands[0]).Equals(sheet.GetEntry("B2")));
        WriteLine(sheet.EvaluateFormulaHelper(f, f.operands[0], sheet.GetEntry("B2")));
        // int i = 0; int j = 0;
        // foreach (List<Entry> l in sheet.entries) {
        //     foreach(Entry e in l) {
        //         sheet.entries[i][j++].content = sheet.EvaluateCell(e);
        //     }
        //     i++;
        //     j = 0;
        // }

        // OutputWriter ow = new OutputWriter($"./{outputFile}");
        // ow.WriteOutput(sheet);
    }
}

class InputReader
{
    public static StreamReader? OpenStreamReader(string inputFile)
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

class OutputWriter {
    StreamWriter sw;
    public OutputWriter(string fileName) {
        this.sw = new StreamWriter(fileName);
    }

    public void WriteOutput(Sheet s) {
        using (sw) {
            int i = 0;
            foreach (List<Entry> l in s.entries) {
                foreach (Entry e in l) {
                    if (e.col == s.entries[i].Count) {
                        sw.Write(e.content + '\n');
                    }
                    else {
                        sw.Write(e.content + ' ');
                    }
                }
                i++;
            }
        }
        
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
        return counter - 1;
    }

    public bool EntryExists(string colName, string rowName)
    {
        int colNum = ResolveColName(colName);
        int rowNum = int.Parse(rowName) - 1;
        if (rowNum < entries.Count && rowNum >= 0)
        {
            if (colNum < entries[rowNum].Count && colNum >= 0)
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
        return entries[int.Parse(rowName) - 1][ResolveColName(colName)];
    }

    public Formula? ParseFormula(Entry e)
    {
        string? op = Formula.ExtractOp(e.content);
        if (op == null) {
            return null;
        }
        string[] operands = Formula.ExtractOperands(e.content, op);
        Formula f = new Formula(op, operands);

        return f;
    }
    // TODO: remove public
    public string EvaluateFormulaHelper(Formula f, string operand, Entry calledFrom) {
        string row; string col;
        if (f.CheckNameFormat(operand, out col, out row)) {
            if (!EntryExists(col, row)) {
                return "0";
            }
            else {
                Entry temp = GetEntry(operand);
                if (temp.Equals(calledFrom)) {
                    return "#CYCLE";
                }
                string value = EvaluateCell(temp);
                if (value == "[]") {
                    return "0";
                }
                else if (int.TryParse(value, out _)) {
                    return value;
                }
                else {
                    return "#ERROR";
                }
            }
        }
        else {
            return "#FORMULA";
        }
    }

    public string EvaluateFormula(Formula formula, Entry calledFrom)
    {
        int res;
        string strOper1 = EvaluateFormulaHelper(formula, formula.operands[0], calledFrom);
        string strOper2 = EvaluateFormulaHelper(formula, formula.operands[1], calledFrom);
        int oper1; int oper2;

        if (!int.TryParse(strOper1, out oper1)) {
            return strOper1;
        }
        if (!int.TryParse(strOper2, out oper2)) {
            return strOper2;
        }
    
        if (formula.op == "+")
        {
            res = oper1 + oper2;
        }
        else if (formula.op == "-")
        {
            res = oper1 - oper2;
        }
        else if (formula.op == "*")
        {
            res = oper1 * oper2;
        }
        else
        {
            if (oper2 == 0) {
                return "#DIV0";
            }
            res = oper1 / oper2;
        }
        return res.ToString();
    }

    public string EvaluateCell(Entry e)
    {
        if (!e.IsFormula())
        {
            if (int.TryParse(e.content, out _) || e.content == "[]") {
                return e.content;
            }
            else {
                return "#INVVAL";
            }
        }
        else
        {
            Formula? f = ParseFormula(e);
            if (f == null) {
                return "#MISSOP";
            }
            return EvaluateFormula(f, e);
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

    public bool CheckNameFormat(string name, out string colName, out string rowName)
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
    public Row row;
    public int col;
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