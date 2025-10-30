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
   [SerializeField] private bool mainProfile = false;

   private void Awake()
   {
      _userDefaultSprite = avatar.sprite;
      if (mainProfile)
      {
         logoutButton.onClick.AddListener(() =>
         {
            if (GameMgr.Instance.hasGameStarted)
            {
               GameMgr.Instance.InitializeGame(true);
               GameMgr.Instance.hasGameStarted = false;
            }

            NetworkMgr.Instance.Logout();
            avatar.sprite = _userDefaultSprite;
            nicknameText.text = "";
            canvasGroup.alpha = 0.25f;
            logoutButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
         });
      }
      else
      {
         avatar.sprite = _userDefaultSprite;
         nicknameText.text = "";
         canvasGroup.alpha = 1f;
         logoutButton.gameObject.SetActive(false);
      }
   }

   public void Initialize(PlayerData playerData)
   {
     // if(mainProfile) return;
      nicknameText.text = playerData.user.nickname;

   }

   private void OnEnable()
   {
      if(!ClientAPI.IsLogged || !mainProfile) return;
      nicknameText.text = ClientAPI.LoggedUser.nickname;
      // _ = ClientAPI.LoadImageAsync($"{BaseAvatarURL}{ClientAPI.LoggedUser.nickname}&bold=true", avatar);
   }

   public void Clear()
   {
      avatar.sprite = _userDefaultSprite;
      nicknameText.text = "";
   }

}
