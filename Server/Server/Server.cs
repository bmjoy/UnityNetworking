using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class Server
    {
        
        public static int MaxPlayers = 10;
        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        
        public delegate void DelegatePacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, DelegatePacketHandler> packetHandlers;

        private static void Update()
        {
            while (true)
            {
                ThreadManager.UpdateMain();
            }
        }

        public static void StartServer(int port)
        {
            Initialize();
            
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);
            
            new Thread(Update).Start();
            
            Console.WriteLine($"Server started on port {port}");
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref endPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int _clientID = packet.ReadInt();

                    if (_clientID == 0)
                    {
                        return;
                    }

                    if (clients[_clientID].GetUDP().endPoint == null)
                    {
                        clients[_clientID].GetUDP().Connect(endPoint);
                        return;
                    }

                    if (clients[_clientID].GetUDP().endPoint.ToString() == endPoint.ToString())
                    {
                        clients[_clientID].GetUDP().HandleData(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        public static void SendUDPData(IPEndPoint endPoint, Packet packet)
        {
            try
            {
                if (endPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending UDP data: {ex}");
            }
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptSocket(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"{client.Client.RemoteEndPoint} just connected");
            
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].GetTCP().socket == null) clients[i].GetTCP().Connect(client);
                return;
            }
            
            Console.WriteLine("User failed to join.");
            Console.WriteLine("Reason: Server full");
        }

        private static void Initialize()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, DelegatePacketHandler>()
            {
                { (int) ClientPackets.WelcomeReceived, PacketHandler.WelcomeReceived },
            };
        }
    }
}