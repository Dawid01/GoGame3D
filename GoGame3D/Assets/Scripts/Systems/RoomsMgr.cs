using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class RoomsMgr : Singleton<RoomsMgr>
{
    
    public void CreateRoom(string roomName)
    {
        _ = CreateRoomTask(roomName);
    }

    public async Task CreateRoomTask(string roomName)
    {
        CreateRoomRequest request = new CreateRoomRequest(NetworkMgr.Instance.currentPlayer.sessionId, roomName);
        UIMgr.Instance.ShowLoadingPanel();

        await ClientAPI.CallPost<Room, CreateRoomRequest>(
            "/rooms/create",
            request,
            (room, response) => {
                UIMgr.Instance.HideLoadingPanel();
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Room created: " + room.RoomName + " " + room.RoomId);
                    _ = JoinRoom(room.RoomId);
                } 
            },
            () => {
                UIMgr.Instance.HideLoadingPanel();
            }
        );
    }
    
    public async Task JoinRoom(String roomId)
    {
        await ClientAPI.CallPost<object, JoinRoomRequest>(
                "/rooms/join",
                new JoinRoomRequest (roomId, NetworkMgr.Instance.currentPlayer.sessionId),
                (obj, response) => {
                    if (response.IsSuccessStatusCode)
                    {
                       // NetworkMgr.Instance.JoinPlayer(roomId);
                       UIMgr.Instance.ActiveElement("Room Panel");
                    }
                },
                () => {
                   
                }
            );

    }
    
    
    [Serializable]
    public class CreateRoomRequest {
        [JsonProperty("sessionId")]
        public String SessionId { get; set; }
        [JsonProperty("roomName")]
        public String RoomName { get; set; }

        public CreateRoomRequest(string sessionId, string roomName)
        {
            SessionId = sessionId;
            RoomName = roomName;
        }
    }

    [Serializable]
    public class JoinRoomRequest {
        [JsonProperty("roomId")]
        public String RoomId { get; set; }
        
        [JsonProperty("sessionId")]
        public String SessionId { get; set; }

        public JoinRoomRequest(string roomId, string sessionId)
        {
            RoomId = roomId;
            SessionId = sessionId;
        }
    }
}
