using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    
    [SerializeField] TMP_InputField createRoomNameInputField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text roomNameTMP;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private Button exitGameButton;

    public int maxPlayers = 4;
    public int minPlayers = 2;
    
    private void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        Debug.Log("Connecting to Master...");
        PhotonNetwork.ConnectUsingSettings();
        exitGameButton.onClick.AddListener(CloseGame);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Master!");
        PhotonNetwork.JoinLobby();
        
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined to Lobby!");

        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(createRoomNameInputField.text))
        {
            return;
        }
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount >= maxPlayers)
        {
            errorText.text = "Room is full. Try again later.";
            MenuManager.Instance.OpenMenu("error");
            return;
        }
        PhotonNetwork.CreateRoom(createRoomNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
        roomNameTMP.text = PhotonNetwork.CurrentRoom.Name;
        
        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.GameObject());
        }
        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }
        
        UpdateStartGameButton();
    }
    
    private void UpdateStartGameButton()
    {
        bool isRoomMaster = PhotonNetwork.IsMasterClient;
        int currentPlayerCount = PhotonNetwork.PlayerList.Length;

        startGameButton.SetActive(isRoomMaster && currentPlayerCount >= minPlayers && currentPlayerCount <= maxPlayers);
    }
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateStartGameButton();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateStartGameButton();
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode == (short)ErrorCode.GameFull)
        {
            errorText.text = "Room is full. Try again later.";
            MenuManager.Instance.OpenMenu("error");
        }
        else
        {
            errorText.text = "Join Room Failed: " + message;
            MenuManager.Instance.OpenMenu("error");
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }
    
    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].RemovedFromList)
                continue;
            
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
        UpdateStartGameButton();
    }
    
    public void CloseGame()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        
        Application.Quit();
    }
}
