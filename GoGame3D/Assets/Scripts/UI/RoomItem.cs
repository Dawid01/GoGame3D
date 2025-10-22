using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class RoomItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private Button joinButton;

    public void Initialize(Room room)
    {
        roomNameText.text = room.name;
        playersText.text = "";
        
        joinButton.onClick.AddListener(() =>
        {
            // ClientAPI.CallPost("/rooms/create", new CreateRoomRequest(), OnSuccessfull: () =>
            // {
            //
            // }, () =>
            // {
            //     
            // });
        });
    }
}



