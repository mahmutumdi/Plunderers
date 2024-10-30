using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MultiplayerGameManager : MonoBehaviourPunCallbacks
{
    public float gameTime = 30f;
    public float countdownTime = 3f;

    private float countdownStartTime;
    private float gameStartTime;
    private bool gameEnded = false;
    private bool countdownCompleted = false;
    private bool gameStarted = false;

    [SerializeField] private Camera mainCamera;
    public TMP_Text timerText;
    public TMP_Text countdownText;
    public TMP_Text gameOverText;
    public Button mainMenuButton;

    [SerializeField] private GameObject playerScoreListItemPrefab;
    [SerializeField] private Transform scoreListParent;
    [SerializeField] private GameObject scoreboardPanel;

    private Dictionary<Player, int> playerScores = new Dictionary<Player, int>();
    public static bool canAct = false;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            countdownStartTime = (float)PhotonNetwork.Time;
            photonView.RPC("SetCountdownStartTime", RpcTarget.AllBuffered, countdownStartTime);
        }

        SetupUI();
        canAct = false;
    }

    private void Update()
    {
        if (gameEnded) return;

        float elapsedTime = (float)PhotonNetwork.Time - countdownStartTime;

        if (!countdownCompleted)
        {
            HandleCountdown(elapsedTime);
        }
        else if (gameStarted)
        {
            HandleGameTimer();
        }
    }

    private void SetupUI()
    {
        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(false);

        gameOverText.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void HandleCountdown(float elapsedTime)
    {
        float remainingCountdown = countdownTime - elapsedTime;

        if (remainingCountdown > 0)
        {
            countdownText.text = Mathf.Ceil(remainingCountdown).ToString();
        }
        else
        {
            countdownText.gameObject.SetActive(false);
            countdownCompleted = true;

            if (PhotonNetwork.IsMasterClient)
            {
                gameStartTime = (float)PhotonNetwork.Time;
                photonView.RPC("StartGame", RpcTarget.AllBuffered, gameStartTime);
            }
        }
    }

    private void HandleGameTimer()
    {
        float remainingGameTime = gameTime - ((float)PhotonNetwork.Time - gameStartTime);
        timerText.text = $"{Mathf.Max(0, Mathf.Ceil(remainingGameTime))}s";

        if (remainingGameTime <= 0 && PhotonNetwork.IsMasterClient)
        {
            EndGame();
        }
    }

    [PunRPC]
    private void SetCountdownStartTime(float networkCountdownStartTime)
    {
        countdownStartTime = networkCountdownStartTime;
    }

    [PunRPC]
    private void StartGame(float networkGameStartTime)
    {
        gameStartTime = networkGameStartTime;
        gameStarted = true;
        canAct = true;
    }

    private void EndGame()
    {
        gameEnded = true;
        timerText.text = "Time: 0s";

        CollectPlayerScores();
        photonView.RPC("CollectAndDisplayScores", RpcTarget.All);
        photonView.RPC("ShowGameOverUI", RpcTarget.All);
    }

    [PunRPC]
    private void ShowGameOverUI()
    {
        timerText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = "Game Ended!";
        mainMenuButton.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(true);

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(true);

        photonView.RPC("DisablePlayerCameras", RpcTarget.All);
    }

    [PunRPC]
    private void DisablePlayerCameras()
    {
        foreach (var playerCamera in GameObject.FindGameObjectsWithTag("PlayerCamera"))
        {
            PhotonView view = playerCamera.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                playerCamera.SetActive(false);
            }
        }
    }

    public void ReturnToMainMenu()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.CleanUpOnLeave();
        }
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CollectAndDisplayScores();
    }

    public override void OnLeftRoom()
    {
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        SceneManager.LoadScene("WaitingScene");
    }

    [PunRPC]
    private void CollectAndDisplayScores()
    {
        CollectPlayerScores();
        DisplayPlayerScores();
    }

    private void CollectPlayerScores()
    {
        playerScores.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObj in players)
        {
            PhotonView pv = playerObj.GetComponent<PhotonView>();
            PlayerInventory inventory = playerObj.GetComponent<PlayerInventory>();

            if (pv != null && inventory != null)
            {
                Player player = pv.Owner;
                playerScores[player] = inventory.GetTotalScore();
            }
        }
    }

    private void DisplayPlayerScores()
    {
        if (scoreListParent == null)
        {
            Debug.LogError("Score list parent is not assigned!");
            return;
        }

        foreach (Transform child in scoreListParent)
        {
            Destroy(child.gameObject);
        }

        var sortedScores = playerScores.OrderByDescending(x => x.Value).ToList();
        foreach (var playerScore in sortedScores)
        {
            if (playerScoreListItemPrefab != null)
            {
                GameObject scoreItem = Instantiate(playerScoreListItemPrefab, scoreListParent);
                PlayerScoreListItem item = scoreItem.GetComponent<PlayerScoreListItem>();

                if (item != null)
                {
                    item.SetPlayerScore(playerScore.Key.NickName, playerScore.Value);
                }
                else
                {
                    Debug.LogError("PlayerScoreListItem component not found on prefab!");
                }
            }
            else
            {
                Debug.LogError("Player score list item prefab is not assigned!");
            }
        }

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(true);
    }
}