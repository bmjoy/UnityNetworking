namespace Client
{
    public class ClientSend
    {

        private static void SendTCPData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.GetTCP().SendData(packet);
        }

        private static void SendUDPData(Packet packet)
        {
            packet.WriteLength();
            ClientObject.Instance.client.GetUDP().SendData(packet);
        }

        public static void WelcomeReceived()
        {
            using (Packet packet = new Packet((int) ClientPackets.WelcomeReceived))
            {
                packet.Write(ClientObject.Instance.client.GetID());
                packet.Write(ClientObject.Instance.client.GetNickname());
                
                SendTCPData(packet);
            }
        }
    }
}