using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Linq;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    PhotonView photonView;
    private PlayerInventory playerInventory;
    private Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(2, 0, 2),
        new Vector3(-2, 0, -2),
        new Vector3(4, 0, -4)
    };
    
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        playerInventory = GetComponent<PlayerInventory>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        // Debug.Log("Instantiated Player Controller!");
        // PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PirateFullController"), Vector3.zero, Quaternion.identity);
        
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber % spawnPositions.Length;
        Vector3 spawnPosition = spawnPositions[playerIndex];

        Debug.Log("Instantiated Player Controller at position: " + spawnPosition);
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PirateFullController"), spawnPosition, Quaternion.identity);
    }
    
    public void DropOffWood()
    {
        if (playerInventory == null)
        {
            GameObject playerController = GameObject.FindWithTag("Player");
            if (playerController != null)
            {
                playerInventory = playerController.GetComponent<PlayerInventory>();
            }
        }

        if (playerInventory != null)
        {
            int woodCount = playerInventory.GetWoodCount();
            int pointsPerWood = playerInventory.GetWoodPoints();
            int totalPoints = woodCount * pointsPerWood;

            if (woodCount > 0)
            {
                playerInventory.AddPoints(totalPoints);
                playerInventory.ResetWoodCount();
                Debug.Log("Wood delivered to base. Points awarded: " + totalPoints);
            }
        }
        else
        {
            Debug.LogError("PlayerInventory component not found on the player GameObject.");
        }
    }
}