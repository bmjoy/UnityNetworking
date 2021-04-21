using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public bool isLocalPlayer = false;
    
    public GameObject nametag;

    public void ChangeNametag(string text)
    {
        nametag.GetComponent<TextMesh>().text = text;
    }
}
