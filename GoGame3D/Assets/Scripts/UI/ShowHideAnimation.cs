using System;
using DG.Tweening;
using UnityEngine;

public class ShowHideAnimation : MonoBehaviour
{

    [SerializeField] private Vector3 hidePosition;
    [SerializeField] private Vector3 showPosition;
    [HideInInspector] public bool isHiden = true;
    [SerializeField] private Transform arrow;
    private bool _blockInput = false;

    private void OnEnable()
    {
        transform.DOKill();
        transform.localPosition = hidePosition;
        isHiden = false;
        _blockInput = false;
        Show(0.2f);
    }

    [ContextMenu("Show or Hide")]
    public void ShowOrHide()
    {
        if(_blockInput) return;
        isHiden = !isHiden;
        _blockInput = true;
        if (isHiden)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Show(float delay = 0f)
    {
        transform.DOKill();
        transform.DOLocalMove(showPosition, 0.5f).OnComplete(() =>
        {
            _blockInput = false;
        }).SetDelay(delay);
        arrow.localScale = new Vector3(-1f, 1f, 1f);
    }

    public void ForceHide()
    {
        if(isHiden) return;
        isHiden = true;
        Hide();
    }

    private void Hide()
    {
        transform.DOKill();
        transform.DOLocalMove(hidePosition, 0.5f).OnComplete(() =>
        {
            _blockInput = false;
        });
        arrow.localScale = new Vector3(1f, 1f, 1f);
    }
}
