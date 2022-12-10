using System;
using static System.Console;
using System.Collections.Generic;
class Top {
    public static void Main() {
        string line = ReadLine()!;

        Reader r = new Reader(line);
        string[] input = line.Split(' ');
        int? res = OpExpr.ParsePrefix(input);
        if (res != null) {
            WriteLine(res);
        }
    }
}

class OpExpr {
    public static int? ParsePrefix(string[] line) {
        Stack<int> stk = new Stack<int>();
        for (int i = line.Length - 1; i >= 0; i--) {
            if (int.TryParse(line[i], out _)) {
                stk.Push(int.Parse(line[i]));
            }
            else if(line[i].Length == 1 && IsOp(line[i][0]) && line[i][0] != '~') {
                int? res;
                try {
                    res = EvaluateExpr(line[i][0], stk.Pop(), stk.Pop());
                }
                catch (InvalidOperationException) {
                    WriteLine("Format Error");
                    return null;
                }
                if (res == null) {
                    return null;
                }
                stk.Push((int) res);
            }
            else if(line[i][0] == '~') {
                int? res;
                try {
                    res = EvaluateExpr(line[i][0], stk.Pop());
                }
                catch (InvalidOperationException) {
                    WriteLine("Format Error");
                    return null;
                }
                if (res == null) {
                    return null;
                }
                stk.Push((int) res);
            }
            else if (line[i] == " ") {
                continue;
            }
            else {
                WriteLine("Format Error");
                return null;
            }
        }
        if (stk.Count != 1) {
            WriteLine("Format Error");
            return null;
        }
        return stk.Pop();
    }

    public static bool IsOp(char c) {
        return c == '+' || c == '-' || c == '*' || c == '/' || c == '~';
    }

    public static int? EvaluateExpr (char op, int operand1, int operand2 = 0) {
        int res;
        try  {
            if (op == '+') {
                res = checked(operand1 + operand2);
             }
            else if (op == '-') {
                checked{res = operand1 - operand2;}
            }
            else if (op == '*') {
                res = checked(operand1 * operand2);
            }
            else if (op == '/') {
                if (operand2 != 0) {
                    res = checked(operand1 / operand2);
                }
                else {
                    WriteLine("Divide Error");
                    return null;
                }
            }
            else {
                res = -operand1;
            }
            return checked(res);
        }
        catch (OverflowException) {
            WriteLine("Overflow Error");
            return null;
        }
               
    }
}

class Reader {
    string s;
    int pos;

    public Reader(string s) {
        this.s = s;
        this.pos = s.Length - 1;
    }

    public char? Read() {
        if (this.pos < 0) {
            return null;
        }

        char c = s[pos--];
        return c;
    }
}