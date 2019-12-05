using System;
using System.Configuration;
using System.Threading;

namespace Jekal
{
    class Program
    {
        private static bool gameRunning = false;

        static void ConsoleThread()
        {
            bool endGame = false;
            while (!endGame)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        endGame = true;
                    }
                }
            }

            gameRunning = false;
        }

        static void Main(string[] args)
        {
            // Load configuration
            var settings = ConfigurationManager.AppSettings;
            var game = new JekalGame(settings);
            game.Initialize();

            var gameThread = new Thread(new ThreadStart(game.StartGame));
            var consoleThread = new Thread(new ThreadStart(ConsoleThread));

            gameThread.Start();
            consoleThread.Start();

            gameRunning = true;
            while (gameRunning)
            {
                Thread.Sleep(100);
            }

            game.StopGame();
            gameThread.Join();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
