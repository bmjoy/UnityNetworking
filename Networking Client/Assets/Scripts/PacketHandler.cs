using System;
using System.Net;

namespace Client
{
    public class PacketHandler
    {

        public static void Welcome(Packet packet)
        {
            string message = packet.ReadString();
            int id = packet.ReadInt();
            
            Console.WriteLine("Server: " + message);
            ClientObject.Instance.client.SetID(id);
            
            ClientSend.WelcomeReceived();
            ClientObject.Instance.client.GetUDP().Connect(((IPEndPoint)ClientObject.Instance.client.GetTCP().socket.Client.LocalEndPoint).Port);
        }

    }
}