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
        InputReader ir = new InputReader(inputFile);

        if (ir.sr == null) {
            return;
        }

        // int intChar;
        // string content = "";
        // while((intChar = ir.ReadChar()) != -1) {
        //     char c = (char) intChar;
        //     if (!InputReader.IsWhiteSpace(c)) {
        //         content += c;
        //     }
        //     else if (c == '\n') { // starting a new row
        //         if (content != "") {
        //             Entry e = new Entry(content, sheet.entries.Count, colCounter++);
        //             sheet.entries[sheet.entries.Count - 1].Add(e);
        //         }
        //         sheet.entries.Add(new List<Entry>());
        //         content = "";
        //         colCounter = 0;
        //     }
        //     else { // adding new entry to row
        //         if (content != "") {
        //             Entry e = new Entry(content, sheet.entries.Count, colCounter++);
        //             sheet.entries[sheet.entries.Count - 1].Add(e);
        //             content = "";
        //         }
        //     }
        // }
        // if (content != "") {
        //     Entry e = new Entry(content, sheet.entries.Count, colCounter++);
        //     sheet.entries[sheet.entries.Count - 1].Add(e);
        // }
        int row = 0;
        int col = 0;
        string? line;
        while ((line = sr.ReadLine()) != null) {
            if (line == "") {
                continue;
            }
            // WriteLine(line);
            sheet.entries.Add(new List<Entry>());
            foreach (string s in System.Text.RegularExpressions.Regex.Split(line, @"\s+")) {
                WriteLine(s);
                sheet.entries[row].Add(new Entry(s, row, col++));
            }
            col = 0;
            row++;
        }

        if (sheet.entries.Count == 1 && sheet.entries[0].Count == 0) {
            return;
        }

        StreamWriter sw = new StreamWriter($"./{outputFile}");
        int i = 0;
        using (sw) {
            foreach (List<Entry> l in sheet.entries) {
                foreach(Entry e in l) {
                    if (e.col == sheet.entries[i].Count - 1) {
                        sw.Write(sheet.EvaluateCell(e) + '\n');
                    }
                    else {
                        sw.Write(sheet.EvaluateCell(e) + ' ');
                    }
                }
                i++;
            }
        }
    }
}

class InputReader
{
    public StreamReader? sr;
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

    public InputReader(string fileName) {
        try {
            this.sr = new StreamReader($"./{fileName}");
        }
        catch {
            WriteLine("File Error");
            return;
        }
    }

    public int ReadChar()
    {
        int c = sr!.Read();
        return c;
    }

    public static bool IsWhiteSpace(char c) {
        return c == ' ' || c == '\n' || c == '\t' || c == '\r';
    }

    public static string[] SplitRow(string row) {
        return System.Text.RegularExpressions.Regex.Split(row, @"\s+");
    }
}

class Sheet
{
    public List<List<Entry>> entries = new List<List<Entry>>();
    List<Entry> cycles = new List<Entry>();
    public static int ResolveColName(string colName)
    {
        int counter = 0;
        for (int i = 0; i < colName.Length; i++)
        {
            counter += ((int)colName[i] - 64) * (int)(Math.Pow(26, colName.Length - i - 1));
        }
        return counter - 1;
    }

    public Formula? ParseFormula(Entry e, out string message)
    {
        string? op = Formula.ExtractOp(e.content);
        if (op == null) {
            message = "#MISSOP";
            return null;
        }
        Entry[]? operands = Formula.ExtractOperands(this, e.content, op);
        if (operands == null) {
            message = "#FORMULA";
            return null;
        }
        Formula f = new Formula(op, operands);
        message = "";
        return f;
    }

    bool EntryExists(Entry e) {
        if (e.row < entries.Count && e.col < entries[e.row].Count && e.row >= 0 && entries[e.row].Count >= 0) {
            return true;
        } 
        return false;
    }

    public string EvaluateCell(Entry e)
    {
        if (e.evaluated) {
            return e.content;
        }
        else if (!e.IsFormula())
        {
            if (int.TryParse(e.content, out _) || e.content == "[]") {
                e.evaluated = true;
            }
            else {
                e.content = "#INVVAL";
                e.evaluated = true;
            }
            return e.content;
        }
        else
        {
            if (cycles.Contains(e)) {
                return "#CYCLE";
            } 
            else {
                cycles.Add(e);
            }

            string message;
            Formula? f = ParseFormula(e, out message);
            if (f == null) {
                return message;
            }
            
            string operand1; string operand2;
            
            if (f.operands[0].evaluated) {
                operand1 = f.operands[0].content;
            }
            else {
                operand1 = EvaluateCell(f.operands[0]);
            }

            if (f.operands[1].evaluated) {
                operand2 = f.operands[1].content;
            }
            else {
                operand2 = EvaluateCell(f.operands[1]);
            }

            if (!int.TryParse(operand1, out _) && operand1 != "[]") {
                string res1 = operand1 == "#CYCLE" ? operand1 : "#ERROR";
                f.operands[0].content = res1;
                f.operands[0].evaluated = true;
                return res1;
            }
            if (!int.TryParse(operand2, out _) && operand2 != "[]") {
                string res2 = operand2 == "#CYCLE" ? operand2 : "#ERROR";
                f.operands[1].content = res2;
                f.operands[1].evaluated = true;
                return res2;
            }
            cycles = new List<Entry>();
            operand1 = operand1 == "[]" ? "0" : operand1;
            operand2 = operand2 == "[]" ? "0" : operand2;
            e.content = EvaluateFormula(int.Parse(operand1), int.Parse(operand2), f.op!);
            e.evaluated = true;
            return e.content;
        }
    }

    public string EvaluateFormula(int operand1, int operand2, string op) {
        int res;
        if (op == "+")
        {
            res = operand1 + operand2;
        }
        else if (op == "-")
        {
            res = operand1 - operand2;
        }
        else if (op == "*")
        {
            res = operand1 * operand2;
        }
        else
        {
            if (operand2 == 0) {
                return "#DIV0";
            }
            res = operand1 / operand2;
        }
        return res.ToString();
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
                if (c >= 'A' && c <= 'Z')
                {
                    colName += c;
                    counter++;
                }
                else
                {
                    if (counter != 0)
                    {
                        if (c > '9' && c < '0')
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
                if (c > '9' && c < '0')
                {
                    return false;
                }
                rowName += c;
            }
        }
        return true;
    }
}

class Formula : Sheet
{
    public string? op { get; set; }
    public Entry[] operands = new Entry[2];

    public Formula(string op, Entry[] operands)
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

    static bool EntryExists(Sheet s, int row, int col) {
        return row < s.entries.Count && row >= 0 && col < s.entries[row].Count && col >= 0;
    }

    public static Entry[]? ExtractOperands(Sheet s, string formula, string op)
    {
        string[] strOperands = formula.Substring(1).Split(op);
        Entry[] operands = new Entry[2];
        string rowName1; string colName1;
        string rowName2; string colName2;
        if(CheckNameFormat(strOperands[0], out colName1, out rowName1)) {
            int row = int.Parse(rowName1) - 1; 
            int col = ResolveColName(colName1);
            if (EntryExists(s, row, col)) {
                operands[0] = s.entries[row][col];
            }
            else {
                operands[0] = new Entry("0", row, col);
            }
        }
        else {
            return null;
        }

        if(CheckNameFormat(strOperands[1], out colName2, out rowName2)) {
            int row = int.Parse(rowName2) - 1; 
            int col = ResolveColName(colName2);
            if (EntryExists(s, row, col)) {
                operands[1] = s.entries[row][col];
            }
            else {
                operands[1] = new Entry("0", row, col);
            }
        }
        else {
            return null;
        }
        return operands;
    }
}

class Entry : Sheet
{
    public string content;
    public int col;
    public int row;
    public bool evaluated = false;
    public Entry(string content, int row, int col)
    {
        this.content = content; this.row = row; this.col = col;
    }

    public bool IsFormula()
    {
        return this.content.Length >= 6 && this.content[0] == '=';
    }
}