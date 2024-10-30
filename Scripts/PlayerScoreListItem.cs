using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class PlayerScoreListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerScoreText;

    public void SetPlayerScore(string playerName, int playerScore)
    {
        playerNameText.text = playerName;
        playerScoreText.text = playerScore.ToString();
    }
}