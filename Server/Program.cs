using System;
using System.Configuration;
using System.Threading;

namespace Jekal
{
    class Program
    {
        private static bool gameRunning = false;
        private static JekalGame game; 

        static void ConsoleThread()
        {
            bool endGame = false;
            while (!endGame)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                            endGame = true;
                            break;
                        case ConsoleKey.P:
                            game.ShowPlayers();
                            break;
                        case ConsoleKey.G:
                            game.ShowGames();
                            break;
                        default:
                            break;
                    }
                    //if (key.Key == ConsoleKey.Escape)
                    //{
                    //    endGame = true;
                    //}
                    //else if (key.Key == ConsoleKey.P)
                    //{
                    //    game.ShowPlayers();
                    //}
                }
            }

            gameRunning = false;
        }

        static void Main(string[] args)
        {
            // Load configuration
            var settings = ConfigurationManager.AppSettings;
            game = new JekalGame(settings);
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
