using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private PhotonView photonView;
    private int woodCount = 0;
    private int totalScore = 0;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    
    public void CollectWood(int points)
    {
        woodCount++;
        Debug.Log("Collected wood.");
    }
    
    public int GetWoodCount()
    {
        return woodCount;
    }
    
    public void ResetWoodCount()
    {
        woodCount = 0;
    }
    
    public int GetWoodPoints()
    {
        return 5;
    }
    
    public void AddPoints(int points)
    {
        totalScore += points;
        Debug.Log("Total Score: " + totalScore);
    }
    
    public int GetTotalScore()
    {
        return totalScore;
    }
}