using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Client
{
    public class Client
    {
        public static Client Instance;
        
        private string ip;
        private int port;
        public static int dataBufferSize = 4096;
        
        //Information
        private int id;
        private string nickname = "Client";
        private TCP tcp;
        private UDP udp;
        private bool isConnected = false;

        public ClientEvents clientEvents;

        private delegate void DelegatePacketHandler(Packet packet);
        private static Dictionary<int, DelegatePacketHandler> packetHandlers;

        public Client(string ip, int port, string nickname)
        {
            if (Instance == null) Instance = this;
            
            this.ip = ip;
            this.port = port;
            this.nickname = nickname;

            tcp = new TCP(ip, port);
            tcp.clientEvents = clientEvents;
            udp = new UDP();
            udp.clientEvents = clientEvents;
        }
        private void Update()
        {
            while (isConnected) ThreadManager.UpdateMain();
        }
        public void Connect()
        {
            InitializeClientData();

            isConnected = true;
            tcp.Connect();
            Debug.Log("Connected to server.");

            new Thread(new ThreadStart(Update)).Start();
            
            if (clientEvents != null) clientEvents.OnConnect();
        }
        public void Disconnect()
        {
            if (isConnected)
            {
                if (clientEvents != null) clientEvents.OnDisconnect();
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();
                
                Debug.Log("Disconnected from server");
            }
        }
        public void AddClientEventListener(ClientEvents clientEvents)
        {
            this.clientEvents = clientEvents;
            tcp.clientEvents = clientEvents;
            udp.clientEvents = clientEvents;
        }
        public void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, DelegatePacketHandler>()
            {
                { (int)ServerPackets.Welcome, PacketHandler.Welcome },
            };
        }
        
        #region Get Set Methods
        public TCP GetTCP() { return tcp; }
        public UDP GetUDP() { return udp; }
        public int GetID() { return id; }
        public string GetIP() { return GetTCP().socket.Client.RemoteEndPoint.ToString(); }
        public string GetNickname() { return nickname; }
        private bool hasSetID = false;
        public void SetID(int id)
        {
            if (!hasSetID) this.id = id;
            hasSetID = true;
        }
        #endregion
        
        #region TCP
        public class TCP
        {
            public TcpClient socket;
            
            private string ip;
            private int port;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;
            public ClientEvents clientEvents;

            public TCP(string ip, int port)
            {
                this.ip = ip;
                this.port = port;
            }

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize,
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(ip, port, ConnectCallback, socket);
            }
            private void Disconnect()
            {
                Instance.Disconnect();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected) { return; }

                stream = socket.GetStream();
                receivedData = new Packet();
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Instance.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    if (socket.Client.Connected) Debug.Log($"Error receiving data: {ex}");
                    Disconnect();
                }
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
                        if (clientEvents != null) clientEvents.OnMessage(receivedData, PacketType.TCP);
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
                            packetHandlers[packetID](packet);
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
                        if (clientEvents != null) clientEvents.OnSend(packet, PacketType.TCP);
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to send packet: {ex}");
                }
            }
        }
        #endregion

        #region UDP
        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public ClientEvents clientEvents;
            
            public UDP() { endPoint = new IPEndPoint(IPAddress.Parse(Instance.ip), Instance.port); }
            public void Connect(int localPort)
            {
                socket = new UdpClient(localPort);
                
                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet packet = new Packet())
                {
                    SendData(packet);
                }
            }
            public void Disconnect()
            {
                Instance.Disconnect();

                endPoint = null;
                socket = null;
            }
            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(Instance.id);
                    if (socket != null)
                    {
                        if (clientEvents != null) clientEvents.OnSend(packet, PacketType.UDP);
                        socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to send UDP data: {ex}");
                }
            }
            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    if (socket == null) return;
                    
                    byte[] data = socket.EndReceive(result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (data.Length < 4)
                    {
                        Instance.Disconnect();
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception ex)
                {
                    if (socket.Client.Connected) Debug.Log($"Error receiving data using UDP: {ex}");
                    Disconnect();
                }
            }
            private void HandleData(byte[] data)
            {
                using (Packet packet = new Packet(data))
                {
                    int packetLength = packet.ReadInt();
                    data = packet.ReadBytes(packetLength);
                    if (clientEvents != null) clientEvents.OnMessage(packet, PacketType.UDP);
                }
                
                ThreadManager.ExecuteOnMainThread((() =>
                {
                    using (Packet packet = new Packet(data))
                    {
                        int packetID = packet.ReadInt();
                        packetHandlers[packetID](packet);
                    }
                }));
            }
        }
        #endregion

        #region Packet Type Enum
        public enum PacketType
        {
            TCP,
            UDP
        }
        #endregion
    }
}