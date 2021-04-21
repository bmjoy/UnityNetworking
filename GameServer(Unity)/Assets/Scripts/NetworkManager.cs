﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public static NetworkManager Instance;

    [Header("Network Settings")] 
    public int port = 444;
    public int maxPlayers = 10;
    
    [Space]

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
        
        Server.Start(maxPlayers, port);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }
}
