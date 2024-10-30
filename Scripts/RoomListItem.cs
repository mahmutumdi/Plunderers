using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text myTMP;
    
    public RoomInfo roomInfo;
    public void SetUp(RoomInfo info)
    {
        roomInfo = info;
        myTMP.text = info.Name;
    }

    public void OnClick()
    {
        Launcher.Instance.JoinRoom(roomInfo);
    }
}