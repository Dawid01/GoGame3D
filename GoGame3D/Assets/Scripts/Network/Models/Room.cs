using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class Room
{
    [JsonProperty("roomId")]
    public String RoomId { get; set; }
    [JsonProperty("roomName")]
    public String RoomName { get; set; }
    [JsonProperty("players")]
    public List<PlayerData> Players { get; set; }

    public Room(string roomId, string roomName, List<PlayerData> players)
    {
        RoomId = roomId;
        RoomName = roomName;
        Players = players;
    }
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
    [JsonProperty("playerId")]
    public String PlayerId { get; set; }

    public JoinRoomRequest(string roomId, string playerId)
    {
        RoomId = roomId;
        PlayerId = playerId;
    }
}
