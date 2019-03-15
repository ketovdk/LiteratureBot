using System;

namespace DataLogic
{
    class Program
    {
        static void Main(string[] args)
        {
            int idx = 0;
            foreach (var a in DataLogic.GetClearData())
            {
                Console.WriteLine(++idx+ a.Key+"!!!"+a.Value);
            }
            
        }
    }
}
