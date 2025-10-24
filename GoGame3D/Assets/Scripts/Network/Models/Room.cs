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


