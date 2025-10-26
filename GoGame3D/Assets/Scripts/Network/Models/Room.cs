using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class Room
{
    public String roomId;
    public String roomName;
    public HashSet<PlayerData> players;

    public Room(String roomId, String roomName, HashSet<PlayerData> players)
    {
        this.roomId = roomId;
        this.roomName = roomName;
        this.players = players;
    }
}


