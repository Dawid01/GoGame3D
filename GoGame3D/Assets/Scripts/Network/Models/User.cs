using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class User
{

    public long id;
    public String email;
    public String nickname;
    public String role;

    public User(long id, string email, string nickname, string role)
    {
        this.id = id;
        this.email = email;
        this.nickname = nickname;
        this.role = role;
    }
}
