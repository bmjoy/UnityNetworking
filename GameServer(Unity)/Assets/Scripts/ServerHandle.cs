using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ServerHandle
{
    #region Handling Packets
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void ReceiveChat(int id, Packet _packet)
    {
        string nickname = _packet.ReadString();
        string chat = _packet.ReadString();

        ServerSend.DistributeChat(id, nickname, chat);
    }
    
    #region Instantiation
    private static T[] GetAtPath<T> (string path) {
        ArrayList al = new ArrayList();
        string [] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        foreach(string fileName in fileEntries)
        {
            int index = fileName.LastIndexOf("/");
            string localPath = "Assets/" + path;
           
            if (index > 0)
                localPath += fileName.Substring(index);
               
            Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

            if(t != null)
                al.Add(t);
        }
        T[] result = new T[al.Count];
        for(int i=0;i<al.Count;i++)
            result[i] = (T)al[i];
           
        return result;
    }
    public static void InstantiatePrefab(int id, Packet _packet)
    {
        string prefabPath = _packet.ReadString();
        string prefabName = _packet.ReadString();
        NetworkTransform prefabTransform = _packet.ReadTransform();

        GameObject[] prefabs = GetAtPath<GameObject>(prefabPath);
        GameObject prefab = null;
        
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i].name == prefabName.Split('/')[prefabName.Split('/').Length - 1])
            {
                prefab = prefabs[i];
            }
        }
        
        GameObject instantiated = MonoBehaviour.Instantiate(prefab);
        instantiated.transform.position = prefabTransform.position;
        instantiated.transform.rotation = prefabTransform.rotation;
        instantiated.transform.localScale = prefabTransform.scale;
        
        ServerSend.SendInstantiatePacket(prefabPath, prefabName, prefabTransform);
    }
    #endregion
    #endregion
}
