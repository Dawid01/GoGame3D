using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private Button startGameButton;

    
    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Room room = NetworkMgr.Instance.currentRoom;
        roomName.text = room.roomName;
        bool isOwner = room.ownerId == NetworkMgr.Instance.currentPlayer.user.id;
        startGameButton.gameObject.SetActive(isOwner);
    }

    public void LeveRoom()
    {
        gameObject.SetActive(false);
        UIMgr.Instance.ActiveElementAnimation("Game Creator Room");
    }

    public void Ready()
    {
        
    }

    public void StartGame()
    {
        
    }

}
