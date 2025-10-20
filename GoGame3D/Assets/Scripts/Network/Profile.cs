using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
   [SerializeField] private Image avatar;
   [SerializeField] private TextMeshProUGUI nicknameText;
   private const string BaseAvatarURL = "https://avatar.iran.liara.run/username?username=";
   
   private void OnEnable()
   {
      if(!ClientAPI.IsLogged) return;
      nicknameText.text = ClientAPI.LoggedUser.Nickname;
      _ = ClientAPI.LoadImageAsync($"{BaseAvatarURL}{ClientAPI.LoggedUser.Nickname}&bold=true", avatar);
   }
}
