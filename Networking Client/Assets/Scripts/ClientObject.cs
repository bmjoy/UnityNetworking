using System;
using UnityEngine;

namespace Client
{
    public class ClientObject : MonoBehaviour
    {
        public static ClientObject Instance;

        public Client client;

        public string ip;
        public int port;
        public string nickname;

        public void Start()
        {
            Instance = this;

            client = new Client(ip, port, nickname);
            client.Connect();
        }

        public void OnApplicationQuit()
        {
            client.Disconnect();
        }
    }
}