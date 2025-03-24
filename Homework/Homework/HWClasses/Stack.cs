using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HWClasses
{
    internal class Stack
    {
        List<string> strList;

        public int Size
        {
            get { return strList.Count; }
        }

        public string Top
        {
            get
            {
                if (strList.Count == 0)
                {
                    return null;
                }
                return strList[strList.Count - 1];
            }

        }
        public Stack(params string[] str)
        {
            this.strList = new List<string>(str);
        }
        public void Add(string str)
        {
            this.strList.Add(str);
        }
        public string Pop()
        {
            if (strList.Count == 0)
            {
                throw new Exception("The Stack is empty");
            }
            string lastOfStr = strList[strList.Count - 1];
            this.strList.RemoveAt(strList.Count - 1);
            return lastOfStr;
        }
        public void ShowStack()
        {
            for (int i = 0; i < strList.Count; i++)
            {
                Console.WriteLine(strList[i]);
            }
        }
        public static Stack Concat(params Stack[] stack)
        {
            Stack mainStack = new Stack();
            for (int i = 0; i < stack.Length; i++)
            {
                mainStack.Merge(stack[i]);
            }
            return mainStack;
        }
        
    }
}