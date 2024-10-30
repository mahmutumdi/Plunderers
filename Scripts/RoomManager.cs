using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public int maxPlayersInRoom = 4;
    
    public static RoomManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void CleanUpOnLeave()
    {
        Debug.Log("Cleaning up RoomManager.");
        if (Instance == this)
        {
            Destroy(gameObject);
            Instance = null;
            Debug.Log("RoomManager instance destroyed successfully.");
        }
        else
        {
            Debug.LogWarning("RoomManager cleanup called, but Instance is either null or mismatched.");
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (PhotonNetwork.CurrentRoom.PlayerCount > maxPlayersInRoom)
        {
            PhotonNetwork.CloseConnection(newPlayer);
            Debug.Log("Room is full. Player was not allowed to join.");
        }
        else
        {
            Debug.Log("Player joined. Current player count: " + PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }
    
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        PhotonNetwork.CurrentRoom.MaxPlayers = maxPlayersInRoom;
    }
}