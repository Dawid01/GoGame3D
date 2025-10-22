using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class Room : MonoBehaviour
{
    [JsonProperty("roomId")]
    public String RoomId { get; set; }
    [JsonProperty("roomName")]
    private String RoomName { get; set; }
    [JsonProperty("players")]
    private List<PlayerData> Players { get; set; }

    public Room(string roomId, string roomName, List<PlayerData> players)
    {
        RoomId = roomId;
        RoomName = roomName;
        Players = players;
    }
}

[Serializable]
public class CreateRoomRequest {
    [JsonProperty("playerId")]
    public String PlayerId { get; set; }
    [JsonProperty("playerId")]
    public String RoomName { get; set; }

    public CreateRoomRequest(string playerId, string roomName)
    {
        PlayerId = playerId;
        RoomName = roomName;
    }
}

[Serializable]
public class JoinRoomRequest {
    [JsonProperty("roomId")]
    public String RoomId { get; set; }
    [JsonProperty("roomId")]
    public String PlayerId { get; set; }

    public JoinRoomRequest(string roomId, string playerId)
    {
        RoomId = roomId;
        PlayerId = playerId;
    }
}
