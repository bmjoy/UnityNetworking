namespace Server
{
    public class ServerSend
    {
        
        #region Send Data Methods
        private static void SendTCPData(int clientID, Packet packet)
        {
            packet.WriteLength();
            Server.clients[clientID].GetTCP().SendData(packet);
        }

        private static void SendUDPData(int clientID, Packet packet)
        {
            packet.WriteLength();
            Server.clients[clientID].GetUDP().SendData(packet);
        }

        private static void SendTCPDataToEveryone(Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].GetTCP().SendData(packet);
            }
        }
        
        private static void SendTCPDataToEveryoneExcept(int except, Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != except) { Server.clients[i].GetTCP().SendData(packet); }
            }
        }
        
        private static void SendUDPDataToEveryone(Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].GetUDP().SendData(packet);
            }
        }
        
        private static void SendUDPDataToEveryoneExcept(int except, Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != except) { Server.clients[i].GetUDP().SendData(packet); }
            }
        }
        #endregion
        
        #region Packet Declarations
        public static void Welcome(int clientID, string message)
        {
            using (Packet packet = new Packet((int) ServerPackets.Welcome))
            {
                packet.Write(message);
                packet.Write(clientID);

                SendTCPData(clientID, packet);
            }
        }

        public static void SpawnPlayer(int toClient, Player player)
        {
            using (Packet packet = new Packet((int) ServerPackets.SpawnPlayer))
            {
                packet.Write(player.id);
                packet.Write(player.nickname);
                packet.Write(player.position);
                packet.Write(player.rotation);
                
                SendTCPData(toClient, packet);
            }
        }
        #endregion
    }
}