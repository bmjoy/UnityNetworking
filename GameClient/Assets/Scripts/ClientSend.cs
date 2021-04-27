using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    #region TCP and UDP Send Methods
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.Instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.Instance.udp.SendData(_packet);
    }
    #endregion

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.Instance.id);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }
    public static void PlayerMovement(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.Instance.id].transform.rotation);

            SendUDPData(_packet);
        }
    }

    public static void SendChat(string username, string chat)
    {
        using (Packet _packet = new Packet((int)ClientPackets.chat))
        {
            _packet.Write(username);
            _packet.Write(chat);

            SendTCPData(_packet);
        }
    }

    public static void InstantiatePrefab(string prefabFolder, GameObject prefab, NetworkTransform prefabTransform)
    {
        using (Packet _packet = new Packet((int) ClientPackets.instantiate))
        {
            _packet.Write(prefabFolder);
            _packet.Write(Application.dataPath + prefabFolder + prefab.name);
            _packet.Write(prefabTransform);
            
            SendTCPData(_packet);
        }
    }
    #endregion
}