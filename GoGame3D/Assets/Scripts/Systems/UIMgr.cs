using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
struct UIElement
{
    public String name;
    public GameObject element;
}

public class UIMgr : Singleton<UIMgr>
{
    [SerializeField] private UIElement[] _elements;
    [SerializeField] private BoardSizes boardSize;
    public Transform hideTarget;
    public Transform showTarget;
    public RectTransform gameboardShowTarget;
    [SerializeField] private GameObject mobileUI;
    public CanvasGroup clearMoveButton;
    public Image playerColorBorder;
    public Ease showHideEase;
    public bool isMobile;

    [SerializeField] private GameObject[] multiplayerObjects;
    [SerializeField] private Button multiplayerButton;

    public override void Awake()
    {
        base.Awake();
        isMobile = Application.isMobilePlatform;
        //isMobile = true;
        mobileUI.SetActive(isMobile);
        ActiveElement("LoginPanel");
        IsLoggedInitialize();

    }
    
    public void ActiveElement(String name)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            UIElement e = _elements[i];
            e.element.SetActive(e.name.Equals(name));

            if (e.name.Equals(name))
            {
                e.element.transform.position = showTarget.position;
                if (i == 0)
                {
                    SetPlayerBorderColor(StoneColor.BLACK, false);

                }
            }
        }
    }
    public void ActiveElementAnimation(String name)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            UIElement e = _elements[i];
            if (e.name.Equals(name))
            {
                e.element.transform.DOKill();
                e.element.transform.position = hideTarget.position;
                e.element.SetActive(true);
                e.element.transform.DOMove(showTarget.position, 0.25f).SetDelay(0.25f).SetEase(showHideEase);

            }
            else
            {
                if (e.element.transform.position != hideTarget.position)
                {
                    e.element.transform.DOKill();
                    e.element.transform.DOMove(hideTarget.position, 0.25f).OnComplete(() =>
                    {
                        e.element.SetActive(false);
                    });
                }
                else
                {
                    e.element.transform.position = hideTarget.position;
                }
            }

            //e.element.SetActive(e.name.Equals(name));
        }
    }

    public void ActiveElementWithoutCloseOthers(String name)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            UIElement e = _elements[i];
            if (e.name.Equals(name))
            {
                e.element.SetActive(true);
                e.element.transform.position = showTarget.position;
                return;
            }
        }
    }

    public void ShowGameplayMobileInput()
    {
        _elements[_elements.Length - 1].element.SetActive(true);
        _elements[_elements.Length - 1].element.transform.position = showTarget.position;
    }

    public void DisactiveElement(String name)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            UIElement e = _elements[i];
            if (e.name.Equals(name))
            {
                e.element.SetActive(false);
                return;
            }
        }
    }
    
    public void DisactiveElementAnimation(String name)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            UIElement e = _elements[i];
            if (e.name.Equals(name))
            {
                if (e.element.transform.position != hideTarget.position)
                {
                    e.element.transform.DOKill();
                    e.element.transform.DOMove(hideTarget.position, 0.25f).OnComplete(() =>
                    {
                        e.element.SetActive(false);
                    }).SetEase(showHideEase);
                }
                else
                {
                    e.element.transform.position = hideTarget.position;
                }
                return;
            }
        }
    }

    public void OnCreateGame()
    {
       
        DOVirtual.DelayedCall(0.1f, () =>
        {
            string shape = boardSize.boardShapesDropdown.options[boardSize.boardShapesDropdown.value].text;
            string size = $"S{boardSize.boardSizesDropdown.options[boardSize.boardSizesDropdown.value].text.ToUpper()}";
            GameMgr.Instance.currentBoardSize = (BoardSize)Enum.Parse(typeof(BoardSize), size);
            GameMgr.Instance.currentGameMode = (GameMode)Enum.Parse(typeof(GameMode), shape.ToUpper());
            GameMgr.Instance.InitializeGame();
        });
    
    }

    public void OnStartGame()
    {
        GameMgr.Instance.StartGame();
        DisactiveElementAnimation("Game Creator");
        SetPlayerBorderColor(StoneColor.BLACK, true);
    }

    public void Quit()
    {
        Application.Quit();
    }
    
    public void SetPlayerBorderColor(StoneColor color, bool visible)
    {
        playerColorBorder.DOKill();
        Color colorValue = color == StoneColor.WHITE ? Color.white : Color.black;
        colorValue = new Color(colorValue.r, colorValue.g, colorValue.b, visible ? 1f : 0f);
        playerColorBorder.DOColor(colorValue, 0.25f);
    }

    public void EnableClearMoveButton(bool active)
    {
        clearMoveButton.DOKill();
        if (active)
        {
            clearMoveButton.gameObject.SetActive(true);
            clearMoveButton.transform.localScale = Vector3.one;
        }

        clearMoveButton.DOFade(active ? 1f : 0f, 0.25f).OnComplete(() =>
        {
            clearMoveButton.gameObject.SetActive(active);
        });
    }

    public void IsLoggedInitialize()
    {
        
        for (int i = 0; i < multiplayerObjects.Length; i++)
        {
            multiplayerObjects[i].SetActive(ClientAPI.IsLogged);
        }

        multiplayerButton.interactable = ClientAPI.IsLogged;
    }



}
