using UnityEditor;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public bool isLocalPlayer = false;

    public GameObject nametag;
    public GameObject cube;

    void Start()
    {
        if (isLocalPlayer) ClientSend.InstantiatePrefab("Prefabs/", cube, new NetworkTransform(new Vector3(Random.Range(-10, 10), 2, 0), Quaternion.identity, Vector3.one));
    }
    
    public void ChangeNametag(string text)
    {
        nametag.GetComponent<TextMesh>().text = text;
    }
}
