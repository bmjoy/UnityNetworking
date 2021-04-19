using System;

namespace Server
{
    public class Example
    {
        public static void Main(string[] args)
        {
            new Example().Start();
        }

        public void Start()
        {
            Console.Title = "Server";

            Server.StartServer(444);

            Console.ReadKey();
        }
    }
}