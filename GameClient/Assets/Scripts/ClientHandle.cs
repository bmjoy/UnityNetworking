using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ClientHandle : MonoBehaviour
{
    #region Utils

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

    #endregion
    
    #region Handling Packets
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Server: {_msg}");
        Client.Instance.id = _myId;
        ClientSend.WelcomeReceived();

        Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
    }
    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }
    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.players[_id].transform.position = _position;
    }
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation;
    }
    public static void ReceiveChat(Packet _packet)
    {
        string name = _packet.ReadString();
        string chat = _packet.ReadString();
        
        Debug.Log($"{name}: {chat}");
    }
    public static void ReceiveInstantiatePacket(Packet _packet)
    {
        string prefabFolder = _packet.ReadString();
        string prefabName = _packet.ReadString();
        NetworkTransform prefabTransform = _packet.ReadTransform();
        
        GameObject[] gos = GetAtPath<GameObject>(prefabFolder);
        GameObject go = null;

        for (int i = 0; i < gos.Length; i++)
        {
            if (Application.dataPath + prefabFolder + gos[i].name == prefabName)
            {
                go = gos[i];
            }
        }

        GameObject instantiated = Instantiate(go);
        instantiated.transform.position = prefabTransform.position;
        instantiated.transform.rotation = prefabTransform.rotation;
        instantiated.transform.localScale = prefabTransform.scale;
    }
    #endregion
}
