using System;

namespace Hello_Space // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Space!");
            using(Game game = new Game(500, 500))
            {
                game.Run();
            }
        }
    }
}