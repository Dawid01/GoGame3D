using System;
using Newtonsoft.Json;
using UnityEngine;

public class User : MonoBehaviour
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
