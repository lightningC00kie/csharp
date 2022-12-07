using System;
using static System.Console;

class Top {
    public static void Main() {
        string line = ReadLine()!;

        Reader r = new Reader(line);
        int? res = OpExpr.ParsePrefix(r);
        if (res != null) {
            WriteLine(res);
        }
    }
}

class OpExpr {
    public static int? ParsePrefix(Reader r) {
        Stack<int> stk = new Stack<int>();
        char? c;
        string num = "";
        while ((c = r.Read()) != null) {
            if (Char.IsDigit((char) c)) {
                num += c;
                continue;
            }
            else if (IsOp((char) c)) {
                if (c != '~') {
                    int? res;
                    try {
                        res = EvaluateExpr((char) c, stk.Pop(), stk.Pop());
                    }
                    catch (InvalidDataException) {
                        WriteLine("Format Error");
                        return null;
                    }
                    if (res == null) {
                        return null;
                    }
                    stk.Push((int) res!); 
                }
                else {
                    int? res;
                    try {
                        res = EvaluateExpr((char) c, stk.Pop());
                    }
                    catch (InvalidOperationException) {
                        WriteLine("Format Error");
                        return null;
                    }
                    if (res == null) {
                        return null;
                    }
                    stk.Push((int) res!);
                }
            }
            if (num.Length != 0) {
                stk.Push(checked(int.Parse(num)));
            }
            num = "";
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
        if (op == '+') {
            res = operand1 + operand2; 
        }
        else if (op == '-') {
            try {
                return checked(operand1 - operand2);
            }
            catch(OverflowException) {
                WriteLine("Overflow Error");
                return null;
            }
        }
        else if (op == '*') {
            res = operand1 * operand2;
        }
        else if (op == '/') {
            if (operand2 != 0) {
                res = operand1 / operand2;
            }
            else {
                WriteLine("Divide Error");
                return null;
            }
        }
        else {
            res = -operand1;
        }
        return res;
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