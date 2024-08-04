using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;

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
    public Ease showHideEase;
    public void ActiveElement(String name)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            UIElement e = _elements[i];
            e.element.SetActive(e.name.Equals(name));
            if (e.name.Equals(name))
            {
                e.element.transform.position = showTarget.position;
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
        string shape = boardSize.boardShapesDropdown.options[boardSize.boardShapesDropdown.value].text;
        string size = $"S{boardSize.boardSizesDropdown.options[boardSize.boardSizesDropdown.value].text.ToUpper()}";
        GameMgr.Instance.currentBoardSize = (BoardSize)Enum.Parse(typeof(BoardSize), size);
        GameMgr.Instance.currentGameMode = (GameMode)Enum.Parse(typeof(GameMode), shape.ToUpper());
        GameMgr.Instance.InitializeGame();
    }

    public void OnStartGame()
    {
        GameMgr.Instance.hasGameStarted = true;
        DisactiveElement("Game Creator");

    }
}
