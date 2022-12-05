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
                
        int colCounter = 0;
        int intChar;
        string content = "";
        sheet.entries.Add(new List<Entry>());
        while((intChar = ir.ReadChar()) != -1) {
            char c = (char) intChar;
            if (!InputReader.IsWhiteSpace(c)) {
                content += c;
            }
            else if (c == '\n' || c == '\r') { // starting a new row
                if (content != "") {
                    Entry e = new Entry(content, sheet.entries.Count, colCounter++);
                    sheet.entries[sheet.entries.Count - 1].Add(e);
                }
                sheet.entries.Add(new List<Entry>());
                content = "";
                colCounter = 0;
            }
            else { // adding new entry to row
                if (content != "") {
                    Entry e = new Entry(content, sheet.entries.Count, colCounter++);
                    sheet.entries[sheet.entries.Count - 1].Add(e);
                    content = "";
                }
            }
        }


        if (content != "") {
            Entry e = new Entry(content, sheet.entries.Count, colCounter++);
            sheet.entries[sheet.entries.Count - 1].Add(e);
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
}

// class OutputWriter {
//     StreamWriter sw;
//     public OutputWriter(string fileName) {
//         this.sw = new StreamWriter(fileName);
//     }

//     public void WriteOutput(Sheet s) {
//         using (sw) {
//             int i = 0;
//             foreach (List<Entry> l in s.entries) {
//                 foreach (Entry e in l) {
//                     if (e.col == s.entries[i].Count) {
//                         sw.Write(e.content + '\n');
//                     }
//                     else {
//                         sw.Write(e.content + ' ');
//                     }
//                 }
//                 i++;
//             }
//         }
        
//     }
// }

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

    // public Entry GetEntry(string name)
    // {
    //     string colName = "";
    //     string rowName = "";
    //     return entries[int.Parse(rowName) - 1][ResolveColName(colName)];
    // }

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

    string EvaluateFormulaHelper(Entry e, Formula f, string operand) {
        string row; string col;
        if (CheckNameFormat(operand, out col, out row)) {
            if (!EntryExists(col, row)) {
                return "0";
            }
            else {
                if (IsCycle(e, f, row, col)) {
                    return "#CYCLE";
                }
                
                Entry temp = entries[int.Parse(row) - 1][ResolveColName(col)];
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

    bool IsCycle(Entry e, Formula f, string row, string col) {
        Entry operEntry = entries[int.Parse(row)-1][ResolveColName(col)];
        if (operEntry.IsFormula()) {
            Formula? f2 = ParseFormula(operEntry);
            // if (f2 != null) {
            //     string operRow; string operCol;
            //     if (!CheckNameFormat(f2.operands[0], out operCol, out operRow)) {
            //         return false;
            //     }

            //     if (ResolveColName(operCol) == e.col && int.Parse(operRow) == e.row) {
            //         return true;
            //     }

            //     if (!CheckNameFormat(f2.operands[1], out operCol, out operRow)) {
            //         return false;
            //     }

            //     if (ResolveColName(operCol) == e.col && int.Parse(operRow) == e.row) {
            //         return true;
            //     }

            // }
            
        }
        return false;
    }

    public string EvaluateFormula(Entry e, Formula formula)
    {
        int res;
        int oper1; int oper2;

        string strOper1 = EvaluateFormulaHelper(e, formula, formula.operands[0]);
        if (!int.TryParse(strOper1, out oper1)) {
            return strOper1;
        }

        string strOper2 = EvaluateFormulaHelper(e, formula, formula.operands[1]);
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
            return EvaluateFormula(e, f);
        }
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

    public static string[] ExtractOperands(string formula, string op)
    {
        string[] operands = formula.Substring(1).Split(op);
        return operands;
    }
}

class Entry : Sheet
{
    public string content;
    public int col;
    public int row;
    public Entry(string content, int row, int col)
    {
        this.content = content; this.row = row; this.col = col;
    }

    public bool IsFormula()
    {
        return this.content.Length >= 6 && this.content[0] == '=';
    }
}