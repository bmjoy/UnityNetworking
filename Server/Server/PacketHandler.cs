using System;

namespace Server
{
    public class PacketHandler
    {

        public static void WelcomeReceived(int clientID, Packet packet)
        {
            int id = packet.ReadInt();
            string username = packet.ReadString();
            
            Console.WriteLine($"{Server.clients[id].GetTCP().socket.Client.RemoteEndPoint} connected is now client {clientID}");
            if (clientID != id) Console.WriteLine("Error assigning ids to clients.");
            
            Server.clients[clientID].SendIntoGame(username);
        }
    }
}