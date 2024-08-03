using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BackgroundRandom : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Image[] backgroundButtons;
    [SerializeField] private Image[] backgroundButtons2;

    private Color _unselectedColor;
    void Awake()
    {
        ColorUtility.TryParseHtmlString("#6D4610", out _unselectedColor);
        UpdateBackground();

    }

    void UpdateBackground()
    {
        int backgroundIndex = PlayerPrefs.GetInt("background", 0);
        for (int i = 0; i < backgroundButtons.Length; i++)
        {
            backgroundButtons[i].color = (i == backgroundIndex) ? Color.green : _unselectedColor;
            backgroundButtons2[i].color = (i == backgroundIndex) ? Color.green : _unselectedColor;

        }
        
        spriteRenderer.sprite = sprites[backgroundIndex];
    }

    public void SetBackground(int index)
    {
        PlayerPrefs.SetInt("background", index);
        UpdateBackground();
    }

    private void OnGameInitialize()
    {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
    }

    private void OnDestroy()
    {
        GameMgr.OnGameInitialize -= OnGameInitialize;
    }

    void Update()
    {
        
    }
}
