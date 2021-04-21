using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        bool local = false;
        if (_id == Client.Instance.id)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            local = true;
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            local = false;
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        _player.GetComponent<PlayerManager>().isLocalPlayer = local;
        if (!local) _player.GetComponent<PlayerManager>().ChangeNametag(_username);
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}
