using System;
using System.Collections.ObjectModel;

namespace Hello_Space
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Space!");

            using Game game = new Game(1366, 768, 120);
            game.Run();
        }
    }
}