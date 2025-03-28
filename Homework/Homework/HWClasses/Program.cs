namespace HWClasses
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Stack strings = new Stack("a", "b", "c");

            Console.WriteLine($"size = {strings.Size}, Top = '{strings.Top}'");

            var deleted = strings.Pop();

            Console.WriteLine($"Remove the top element '{deleted}' size = {strings.Size}");

            strings.Add("d");

            Console.WriteLine($"size = {strings.Size}, Top = '{strings.Top}'");

            strings.Pop();
            strings.Pop();
            strings.Pop();

            Console.WriteLine($"size = {strings.Size}, Top = '{(strings.Top == null ? "null" : strings.Top)}'");

            Stack strings2 = new Stack("e", "f", "g");

            Stack strings3 = new Stack("h", "i", "j");

            strings2.Merge(strings3);
            strings2.ShowStack();

            Console.WriteLine();

            Stack strings4 = new Stack();
            strings4 = Stack.Concat(new Stack("1","2","3"), new Stack("A","B","C"), new Stack ("4","5","6"));

            strings4.ShowStack();
            Console.WriteLine(strings4);




        }
    }
}
