using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class Room
{
    [ReadOnly] public String roomId;
    [ReadOnly] public String roomName;
    [ReadOnly] public long ownerId;
    public List<PlayerData> players;

    public Room(String roomId, String roomName, List<PlayerData> players)
    {
        this.roomId = roomId;
        this.roomName = roomName;
        this.players = players;
    }
}


