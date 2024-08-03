using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public void ActiveElement(String name)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            UIElement e = _elements[i];
            e.element.SetActive(e.name.Equals(name));
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

    public void OnCreateGame()
    {
        string shape = boardSize.boardShapesDropdown.options[boardSize.boardShapesDropdown.value].text;
        string size = $"S{boardSize.boardSizesDropdown.options[boardSize.boardSizesDropdown.value].text.ToUpper()}";
        GameMgr.Instance.currentBoardSize = (BoardSize)Enum.Parse(typeof(BoardSize), size);
        GameMgr.Instance.currentGameMode = (GameMode)Enum.Parse(typeof(GameMode), shape.ToUpper());
        GameMgr.Instance.InitializeGame();
    }
}
