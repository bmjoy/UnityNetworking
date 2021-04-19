namespace Client
{
    public interface ClientEvents
    {

        void OnConnect();
        void OnDisconnect();
        void OnMessage(Packet packet, Client.PacketType packetType);
        void OnSend(Packet packet, Client.PacketType packetType);

    }
}