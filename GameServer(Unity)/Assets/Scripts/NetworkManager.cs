using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public static NetworkManager Instance;

    public GameObject playerPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance);
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        #if UNITY_EDITOR
        Debug.Log("Build the server before playing.");
        #else
        Server.Start(10, 444);
        #endif
    }
}
