using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public enum StoneColor
{
    WHITE = 1, BLACK =2
}

public class Stone : PoolableObject
{
    public StoneColor stoneColor;
    [SerializeField] private MeshRenderer meshRenderer;

    public Stone Initialize(StoneColor stoneColor)
    {
        this.stoneColor = stoneColor;
        meshRenderer.sharedMaterial = stoneColor == StoneColor.WHITE ? GameMgr.Instance.whiteMaterial : GameMgr.Instance.blackMaterial;
        return this;
    }

    public void Show()
    {
        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.25f).SetEase(Ease.OutBounce);
    }
    
    public void Hide(Action OnComplete = null, float delay = 0f)
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOScale(0f, 0.25f).SetEase(Ease.InBounce).OnComplete(() =>
        {
            AudioMgr.Instance.PlayAudio(AudioMgr.Instance.dropAudioClip);
            OnComplete?.Invoke();
            Despawn();
        }).SetDelay(delay);
    }

    public void Shake()
    {
        transform.DOKill(true);
        transform.DOShakePosition(0.25f, new Vector3(1f, 0f, 1f) * 0.1f, 15, 90f);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
    }
}
