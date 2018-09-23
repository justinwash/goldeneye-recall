using System;
using Screen;

namespace GoldenEyeRecall
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pixel @ coordinates:");
            var p = new Pixel();
            p.CheckForReset("obs64", 600, 300);
        }
    }
}
