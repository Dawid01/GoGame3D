using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour, IPanel
{
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Profile[] profiles;

    public void SubscribeEvents()
    {
        NetworkMgr.OnPlayerJoin += JoinPlayer;
        NetworkMgr.OnPlayerLeave += LeavePlayer;
    }

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
    
    private void JoinPlayer(PlayerData playerData)
    {
        Debug.Log("Player joined: " + playerData.user.nickname);
        if(NetworkMgr.Instance.currentRoom == null) return;
        NetworkMgr.Instance.currentRoom.players.Add(playerData);
        UpdateProfiles();
        // Profile profile = playerData.user.id == NetworkMgr.Instance.currentRoom.ownerId
        //     ? profiles[0]
        //     : profiles[1];
        // profile.Initialize(playerData);

    }

    private void UpdateProfiles()
    {
        for (int i = 0; i < profiles.Length; i++)
        {
            Profile profile = profiles[i];
            Room room = NetworkMgr.Instance.currentRoom;
            if (i < room.players.Count)
            {
                PlayerData playerData = room.players[i];
                profile.Initialize(playerData);
            }
            else
            {
                profile.Clear();
            }
        }
    }


    private void LeavePlayer(PlayerData playerData)
    {
        if(NetworkMgr.Instance.currentRoom == null) return;
        Debug.Log("Player left: " + playerData.user.nickname);
    }
    
    private void OnDestroy()
    {
        NetworkMgr.OnPlayerJoin -= JoinPlayer;
        NetworkMgr.OnPlayerLeave -= LeavePlayer;
    }

}
