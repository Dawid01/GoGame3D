using System;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class CreateRoomPanel : MonoBehaviour
{

    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TextMeshProUGUI nameError;
    [SerializeField] private TextMeshProUGUI characterLimit;
    [SerializeField] private Profile ownerProfile;
    [SerializeField] private Profile guestProfile;

    private void Awake()
    {
        Clear();
        nameField.onValueChanged.AddListener((value) =>
        {
            characterLimit.text = $"{value.Length}/{nameField.characterLimit}";
        });
        NetworkMgr.onPlayerJoin += JoinPlayer;
        NetworkMgr.onPlayerLeave += LeavePlayer;
    }

    public void CreateRoom()
    {
        if(!ClientAPI.IsLogged) return;
        string name = nameField.text;
        bool isError = false;

        if (string.IsNullOrWhiteSpace(name))
        {
            isError = true;
            nameError.gameObject.SetActive(true);
            nameError.text = "Name field is empty!";
        }
        
        if(isError) return;
        RoomsMgr.Instance.CreateRoom(name);
    }

    private void Clear()
    {
        nameField.text = "";
        nameError.text = "";
        characterLimit.text = $"0/{nameField.characterLimit}";
    }

    private void JoinPlayer(PlayerData playerData)
    {
        if(NetworkMgr.Instance.currentRoom == null) return;
        Debug.Log("Player joined: " + playerData.user.nickname);
        NetworkMgr.Instance.currentRoom.players.Add(playerData);
        Profile profile = playerData.currentRoomId == NetworkMgr.Instance.currentRoom.roomId
            ? ownerProfile
            : guestProfile;
        profile.Initialize(playerData);

    }
    
    private void LeavePlayer(PlayerData playerData)
    {
        if(NetworkMgr.Instance.currentRoom == null) return;
        Debug.Log("Player left: " + playerData.user.nickname);
    }

    private void OnDisable()
    {
        Clear();
    }

    private void OnDestroy()
    {
        NetworkMgr.onPlayerJoin -= JoinPlayer;
        NetworkMgr.onPlayerLeave -= LeavePlayer;
    }
}
