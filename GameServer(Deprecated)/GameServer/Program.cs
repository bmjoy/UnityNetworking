using System;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(10, 444);
        }

        private static void MainThread()
        {
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
