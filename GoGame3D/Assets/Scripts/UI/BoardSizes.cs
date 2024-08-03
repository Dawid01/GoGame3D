using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSizes : MonoBehaviour
{
    public TMP_Dropdown boardShapesDropdown;
    public TMP_Dropdown boardSizesDropdown;

    private List<string> classicSizes = new List<string>
    {
        "9x9",
        "13x13",
        "19x19"
    };
    private List<string> smallerSizes = new List<string>
    {
        "5x5",
        "7x7",
        "9x9"
    };



    private Dictionary<string, List<string>> smallerSizesDict;

    void Start()
    {
        smallerSizesDict = new Dictionary<string, List<string>> { { "CUBE", smallerSizes }, { "SPHERE", smallerSizes } };

        if (boardShapesDropdown == null)
        {
            Debug.LogError("boardShapesDropdown is not assigned!");
        }
        if (boardSizesDropdown == null)
        {
            Debug.LogError("boardSizesDropdown is not assigned!");
        }

        boardShapesDropdown.onValueChanged.AddListener(OnBoardShapesDropdownChanged);
        UpdateBoardSizesDropdown(boardShapesDropdown.options[boardShapesDropdown.value].text);
    }

    void OnBoardShapesDropdownChanged(int index)
    {
        string selectedSize = boardShapesDropdown.options[index].text;
        UpdateBoardSizesDropdown(selectedSize);
    }

    void UpdateBoardSizesDropdown(string boardShape)
    {
        boardSizesDropdown.ClearOptions();
        List<string> sizes = new List<string>();

        if (smallerSizesDict.ContainsKey(boardShape))
        {
            sizes.AddRange(smallerSizesDict[boardShape]);
        }
        else
        {
            sizes.AddRange(classicSizes);
        }

        boardSizesDropdown.AddOptions(sizes);
    }
}
