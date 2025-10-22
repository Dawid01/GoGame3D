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
   [SerializeField] private Button logoutButton;
   private Sprite _userDefaultSprite;
   [SerializeField] private CanvasGroup canvasGroup;

   private void Awake()
   {
      _userDefaultSprite = avatar.sprite;
      logoutButton.onClick.AddListener(() =>
      {
         if (GameMgr.Instance.hasGameStarted)
         {
            GameMgr.Instance.InitializeGame(true);
            GameMgr.Instance.hasGameStarted = false;
         }

         ClientAPI.Logout();
         NetworkMgr.Instance.websocket.Close();
         avatar.sprite = _userDefaultSprite;
         nicknameText.text = "";
         UIMgr.Instance.ActiveElement("LoginPanel");
         canvasGroup.alpha = 0.25f;
         logoutButton.gameObject.SetActive(false);
         gameObject.SetActive(false);
      });
   }

   private void OnEnable()
   {
      if(!ClientAPI.IsLogged) return;
      nicknameText.text = ClientAPI.LoggedUser.Nickname;
      _ = ClientAPI.LoadImageAsync($"{BaseAvatarURL}{ClientAPI.LoggedUser.Nickname}&bold=true", avatar);
   }
    
}
