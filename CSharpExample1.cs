using System;
using System.Collections.Generic;

//https://www.geeksforgeeks.org/python-program-to-find-uncommon-words-from-two-strings/

namespace FindUncommonWordsFromTwoStrings
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write(String.Join(",",FindUncommonWords("the cat in the hat is fun", "the dog in the car is bad")));    
            System.Threading.Thread.Sleep(1000 * 10);
        }
        public static List<string> FindUncommonWords(string stringA, string stringB)
        {
            string[] arrA = stringA.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] arrB = stringB.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>();
            bool isNotUnique = false;
            foreach (string a in arrA)
            {
                foreach (string b in arrB)
                {
                    if (a == b)
                    {
                        isNotUnique = true;
                        break;
                    }
                }
                if (!isNotUnique)
                {
                    list.Add(a);
                }
                isNotUnique = false;
            }
            foreach (string b in arrB)
            {
                foreach (string a in arrA)
                {
                    if (b == a)
                    {
                        isNotUnique = true;
                        break;
                    }
                }
                if (!isNotUnique)
                {
                    list.Add(b);
                }
                isNotUnique = false;
            }
            return list;
        }
    }
}
