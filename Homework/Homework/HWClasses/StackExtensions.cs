using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HWClasses
{
    internal static class StackExtensions
    {
        public static void Merge(this Stack s1, Stack s2)
        {
            while (s2.Size > 0)
            {
                string lasts2 = s2.Pop();
                s1.Add(lasts2);
            }
        }          
    }
}
