using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class User
{
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("email")]
    public String Email { get; set; }
    [JsonProperty("nickname")]
    public String Nickname { get; set; }
    [JsonProperty("role")]
    public String Role { get; set; }

    public User(long id, string email, string nickname, string role)
    {
        Id = id;
        Email = email;
        Nickname = nickname;
        Role = role;
    }
}
