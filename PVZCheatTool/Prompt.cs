using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvzHakTool
{
    class Prompt
    {
        public static string createInput(string text = "Type Anything", ConsoleColor col = ConsoleColor.Cyan, ConsoleColor col2 = ConsoleColor.Green, ConsoleColor col3 = ConsoleColor.White)
        {
            text += ": ";
            writePrompt(0, text, col3, col2);
            Console.ForegroundColor = col;
            string toRet = Console.ReadLine();
            Console.ForegroundColor = col3;
            return toRet;
        }

        public static string createPassword(string text = "Type Anything", ConsoleColor col = ConsoleColor.Cyan, ConsoleColor col2 = ConsoleColor.Green, ConsoleColor col3 = ConsoleColor.White)
        {
            text += ": ";
            writePrompt(1, text, col3, col2);
            Console.ForegroundColor = col;
            string toRet = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    toRet += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && toRet.Length > 0)
                    {
                        toRet = toRet.Substring(0, (toRet.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (string.IsNullOrWhiteSpace(toRet))
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Empty value not allowed.");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("");
                            break;
                        }
                    }
                }
            } while (true);
            Console.ForegroundColor = col3;
            return toRet;
        }

        public static void writePrompt(int ind, string text, ConsoleColor col, ConsoleColor col2)
        {
            string[] prefixes = { "?", "*" };
            string prefix = prefixes[ind] + " ";
            Console.ForegroundColor = col2;
            Console.Write(prefix);
            Console.ForegroundColor = col;
            Console.Write(text);
        }

        public static bool confirm(string toConfirm)
        {
            if (toConfirm.Length > 0)
            {
                toConfirm = toConfirm.ToLower();
                if (toConfirm[0] == 'n') return false;
                return true;
            }
            else return true;
        }
    }
}
