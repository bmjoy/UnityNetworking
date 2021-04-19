using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace Server
{
    public class Client
    {
        private int id;
        private TCP tcp;
        private UDP udp;

        public Player player;

        public static int dataBufferSize = 4096;

        public Client(int id)
        {
            this.id = id;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} just disconnected.");
            tcp.Disconnect();
            udp.Disconnect();
        }

        public void SendIntoGame(string playerName)
        {
            player = new Player(id, playerName, Vector3.Zero);

            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }

            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(_client.id, player);
                }
            }
        }
        
        #region Get Set Methods
        public int GetID() { return id; }
        public TCP GetTCP() { return tcp; }
        public UDP GetUDP() { return udp; }
        public string GetIP() { return tcp.socket.Client.RemoteEndPoint.ToString(); }
        #endregion

        #region TCP
        public class TCP
        {
            public TcpClient socket;
            public NetworkStream stream;
            private Packet receivedData;
            public readonly int id;

            private byte[] receiveBuffer;
            
            public TCP(int id) { this.id = id; }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                this.socket.SendBufferSize = dataBufferSize;
                this.socket.ReceiveBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                
                ServerSend.Welcome(id, "Welcome to the server!");
            }
            
            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                
                receivedData.SetBytes(data);
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetID = packet.ReadInt();
                            Server.packetHandlers[packetID](id, packet);
                        }
                    });

                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }
                
                if (packetLength <= 1) { return true; }

                return false;
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send TCP data: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving data: {ex}");
                    Server.clients[id].Disconnect();
                }
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }
        #endregion

        #region UDP
        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int id)
            {
                this.id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                this.endPoint = endPoint;
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packet)
            {
                int packetLength = packet.ReadInt();
                byte[] data = packet.ReadBytes(packetLength);
                
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(data))
                    {
                        int packetID = _packet.ReadInt();
                        Server.packetHandlers[packetID](id, _packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }
        #endregion
    }
}