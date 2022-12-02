using System;
using System.IO;
using System.Collections.Generic;
using static System.Console;

class Top
{
    static void Main(String[] args)
    {
        if (args.Length < 3)
        {
            WriteLine("Argument Error");
            return;
        }

        int max_width;

        if (!int.TryParse(args[args.Length - 1], out max_width) || max_width <= 0)
        {
            WriteLine("Argument Error");
            return;
        }

        bool HIGHLIGHT_SPACES = false;

        String OUTPUT = args[args.Length - 2];
        if (args[0] == "--highlight-spaces")
        {
            HIGHLIGHT_SPACES = true;
        }
        List<String> words = new List<string>();
        for (int i = HIGHLIGHT_SPACES ? 1 : 0; i < args.Length - 2; i++)
        {

            if (File.Exists($"./{args[i]}"))
            {
                foreach (String word in read_input(args[i]))
                {
                    words.Add(word);
                }
            }
            // if (i != args.Length - 3)
            // {
            //     words.Add("");
            // }
        }
        write_output(OUTPUT, words, max_width, HIGHLIGHT_SPACES);


    }

    static List<String> read_input(String INPUT_FILE)
    {
        List<String> words = new List<string>();
        try
        {
            using (StreamReader sr = new StreamReader($"./{INPUT_FILE}"))
            {

                char c;
                int c_num;
                bool new_para = false;
                int newlines = 0;
                String s = "";

                while ((c_num = sr.Read()) != -1)
                {
                    c = (char)c_num;
                    if (newlines == 2 && !new_para && words.Count > 0)
                    {
                        new_para = true;
                        words.Add("");
                    }

                    if (!is_white_space(c))
                    {
                        s += c;
                        new_para = false;
                        newlines = 0;
                    }

                    if (is_white_space(c))
                    {
                        if (s != "")
                        {
                            words.Add(s.TrimEnd());
                        }

                        if (c == '\n')
                        {
                            newlines++;
                        }
                        s = "";
                    }
                }

                if (words.Count == 0)
                {
                    return words;
                }
                if (s != "")
                {
                    words.Add(s.TrimEnd());
                }

                if (words[words.Count - 1] == "")
                {
                    words.RemoveAt(words.Count - 1);
                }


            }
        }

        catch (Exception)
        {
            WriteLine("File Error");
        }
        return words;
    }

    static bool is_white_space(char c)
    {
        return c == '\n' || c == '\t' || c == ' ' || c == '\r';
    }

    static int line_length(List<String> l)
    {
        int length = 0;
        foreach (string s in l)
        {
            length = length + s.TrimEnd().Length;
        }
        return length;
    }

    static String form_line(List<String> l, int max_width, bool hs)
    {
        String line = "";
        String white_space = hs ? "." : " ";
        int line_len = line_length(l);
        double to_fill = max_width - line_len;
        int gaps = l.Count - 1;
        foreach (String word in l)
        {
            string gap = "";
            for (int i = 0; i < (int)Math.Ceiling(to_fill / gaps); i++)
            {
                gap += white_space;
            }
            to_fill -= Math.Ceiling(to_fill / gaps);
            gaps--;
            line = line + word.TrimEnd() + gap;
        }
        return line;
    }

    static void write_output(String OUTPUT, List<String> words, int max_width, bool hs)
    {
        List<String> line = new List<string>();
        String newline = hs ? "<-" + Environment.NewLine : Environment.NewLine;
        String white_space = hs ? "." : " ";
        using (StreamWriter sw = new StreamWriter($"./{OUTPUT}", true))
        {
            int counter = 0;
            foreach (string word in words)
            {
                counter++;
                if (word.Length >= max_width)
                {
                    if (line.Count > 0)
                    {
                        sw.Write(form_line(line, max_width, hs) + newline);
                        line = new List<string>();
                    }
                    sw.Write(word + newline);
                    continue;
                }

                if (word == "")
                {
                    if (line.Count > 0)
                    {
                        sw.Write(string.Join(white_space, line) + newline);
                    }
                    sw.Write("" + newline);

                    line = new List<string>();
                }

                if (line_length(line) + line.Count + word.Length > max_width)
                {
                    sw.Write(form_line(line, max_width, hs) + newline);
                    line = new List<string>();
                    line.Add(word);
                }
                else
                {
                    if (word != "")
                        line.Add(word);
                }
            }

            if (line.Count != 0)
            {
                sw.Write(string.Join(white_space, line) + newline);
            }
        }
    }
}


// using static System.Console;
// using System.IO;
// using System;
// using System.Collections.Generic;
// // using System.Math; 
// class Justify
// {
//     static void Main(string[] args)
//     {
//         if (args.Length >= 3 && Int32.TryParse(args[args.Length - 1], out int max_width) && max_width > 0)
//         {
//             string out_file = args[args.Length - 2];
//             // bool all_files_wrong = true; 

//             StreamWriter output = new StreamWriter(out_file);
//             int charCounter = 0;
//             string word = "";
//             string line = "";
//             char spaces = ' ';
//             bool paragraphStarted = false;
//             bool paragraphEnded = true;
//             bool textStarted = false;


//             for (int i = 0; i < args.Length - 2; i++)
//             {
//                 if (i == 0 && args[i] == "--highlight-spaces")
//                 {
//                     spaces = '.';
//                     continue;
//                 }
//                 string inp_file = args[i];
//                 try
//                 {
//                     StreamReader text = new StreamReader(inp_file);
//                     char c;

//                     if (line != "" && !isWhiteSpace((char)text.Peek()))
//                     {
//                         line = line.TrimEnd();
//                     }

//                     while (text.Peek() != -1)
//                     {
//                         c = (char)text.Read()!;
//                         int nextChar = text.Peek();

//                         // we check if paragraph can start 
//                         if (c == '\n')
//                         {
//                             if ((!paragraphStarted && textStarted))
//                             {
//                                 paragraphStarted = true;
//                                 paragraphEnded = false;
//                             }
//                             else if (!paragraphEnded && ((nextChar == -1 && i != args.Length - 3) ||
//                                                 (!isWhiteSpace((char)nextChar) && nextChar != -1)))
//                             {
//                                 paragraphEnded = true; textStarted = false;
//                                 if (line != "")
//                                     output.Write(replaceSpaces(line.TrimEnd(), spaces) +
//                                                                     (spaces == '.' ? "<-" : "") + Environment.NewLine);
//                                 line = ""; charCounter = word.Length;
//                                 output.Write(Environment.NewLine); // found the end of a paragraph 
//                             }

//                         }

//                         //we skip continuous whitespaces that won't help us 
//                         if ((isWhiteSpace(c) && isWhiteSpace((char)nextChar)) ||
//                             (line == "" && isWhiteSpace(c))) continue;

//                         paragraphStarted = false; textStarted = true;
//                         charCounter++;

//                         if (!isWhiteSpace(c))
//                         {
//                             word += c;
//                             //check if the word can end here  
//                             if (isWhiteSpace((char)nextChar) || nextChar == -1)
//                             {
//                                 line += line == "" ? word : $" {word}";
//                                 word = "";

//                             }
//                         }

//                         //write line to the output file if we reached the required width 
//                         if (charCounter >= max_width && line != "")
//                         {
//                             output.Write(editSpaces(line, max_width - line.Length, spaces));
//                             output.Write((spaces == '.' ? "<-" : "") + Environment.NewLine);
//                             line = "";
//                             charCounter = word.Length;
//                         }
//                     }

//                     text.Close();

//                 }
//                 catch (IOException)
//                 {
//                     //treat it as empty file 
//                 }
//             }
//             if (line != "") output.Write(replaceSpaces(line.TrimEnd(), spaces) + (spaces == '.' ? "<-" : "") + Environment.NewLine);
//             output.Close();

//         }
//         else
//         {
//             WriteLine("Argument Error");
//         }
//     }

//     static bool isWhiteSpace(char c)
//     {
//         return (c == ' ' || c == '\n' || c == '\t' || c == '\r');
//     }

//     static string replaceSpaces(string line, char spaces)
//     {
//         string[] words = line.Split();
//         return String.Join(spaces, words);
//     }

//     static string editSpaces(string line, int extra_space, char spaces)
//     {
//         if (extra_space == 0) return replaceSpaces(line, spaces);

//         string[] words = line.Split();
//         int num_words = words.Length;
//         if (num_words <= 1) return line;

//         int spaces_between = extra_space / (num_words - 1);
//         double leftover = (double)extra_space / (double)(num_words - 1) - spaces_between;

//         string outp = "";

//         for (int i = 1; i < num_words; i++)
//         {
//             string extra = new string(spaces, spaces_between + 1);
//             if (leftover != 0 &&
//                 (Math.Round((double)i / (num_words - 1), 6)) <= Math.Round(leftover, 6))
//             {
//                 extra += spaces;
//             }
//             outp += words[i - 1] + extra;
//         }

//         return outp + words[num_words - 1];

//     }

// }