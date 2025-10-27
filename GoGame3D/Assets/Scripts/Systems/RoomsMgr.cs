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
        await ClientAPI.CallPost<String, CreateRoomRequest>(
            "/rooms/create",
            request,
            (id, response) => {
                UIMgr.Instance.HideLoadingPanel();
                Debug.Log("Room created: " + id);
                _ = JoinRoom(id);
            },
            () => {
                UIMgr.Instance.HideLoadingPanel();
            }
        );
    }
    
    public async Task JoinRoom(String roomId)
    {
        await ClientAPI.CallPost<Room, JoinRoomRequest>(
                "/rooms/join",
                new JoinRoomRequest (roomId, NetworkMgr.Instance.currentPlayer.sessionId),
                (room, response) => {
                    if(room == null) return;
                    NetworkMgr.Instance.currentRoom = room;
                    UIMgr.Instance.ActiveElement("Room Panel");
                },
                () => {
                   
                }
            );

    }
    
    
    [Serializable]
    public class CreateRoomRequest
    {
        public String sessionId;
        public String roomName;

        public CreateRoomRequest(string sessionId, string roomName)
        {
            this.sessionId = sessionId;
            this.roomName = roomName;
        }
    }

    [Serializable]
    public class JoinRoomRequest
    {

        public String roomId;
        public String sessionId;

        public JoinRoomRequest(string roomId, string sessionId)
        {
            this.roomId = roomId;
            this.sessionId = sessionId;
        }
    }
}
