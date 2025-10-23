using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PasswordEye : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite hideIcon;
    [SerializeField] private Sprite showIcon;
    
    private bool _hide = true;

    private void OnEnable()
    {
        _hide = true;
        UpdateIcons();
    }

    public void OnClick()
    {
        _hide = !_hide;
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        icon.sprite = _hide ? hideIcon : showIcon;
        inputField.contentType = _hide ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        inputField.textComponent.SetAllDirty();
    }
}
