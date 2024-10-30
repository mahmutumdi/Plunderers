using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BaseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                PlayerManager playerManager = FindObjectOfType<PlayerManager>();
                if (playerManager != null)
                {
                    Debug.Log("Player reached base, triggering DropOffWood");
                    playerManager.DropOffWood();
                }
                else
                {
                    Debug.LogError("PlayerManager component not found in the scene.");
                }
            }
        }
    }
}