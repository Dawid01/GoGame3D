using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShapeDescription : MonoBehaviour
{

    public TMP_Dropdown boardShapesDropdown;

    public TMP_Text contextText;

    public Sprite[] boards;
    public Image boardImage;
    
    void Start()
    {

        if (boardShapesDropdown == null)
        {
            Debug.LogError("boardShapesDropdown is not assigned!");
        }
        boardShapesDropdown.onValueChanged.AddListener(delegate { OnBoardShapesDropdownChanged(boardShapesDropdown); });
    }

    // Update is called once per frame
    void OnBoardShapesDropdownChanged(TMP_Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                //contextText.text = "Normal board";
                boardImage.sprite = boards[0];
                break;
            case 1:
                //contextText.text = "Looping board";
                boardImage.sprite = boards[1];
                break;
            case 2:
                //contextText.text = "Cube board";
                boardImage.sprite = boards[2];
                break;
            case 3:
                //contextText.text = "Sphere board";
                boardImage.sprite = boards[3];
                break;
            default:
               // contextText.text = "Nie dziala";
                break;
        }
    }
}
