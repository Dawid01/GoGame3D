using System;
using DG.Tweening;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    private Sequence _sequence;
    [SerializeField] private Transform loadingImg;
    [SerializeField] private Transform[] stones;
    
    public void Initialize()
    {
        if(_sequence != null) return;
        float duration = 2f;
        transform.SetAsLastSibling();
        _sequence = DOTween.Sequence().SetLink(gameObject);
        loadingImg.localEulerAngles = Vector3.zero;
        _sequence.Insert(0, loadingImg.DOLocalRotate(new Vector3(0f, 0f, -360f * 2f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutElastic)
            .OnComplete(
            () =>
            {
                loadingImg.localEulerAngles = Vector3.zero;
            }));

        for (int i = 0; i < stones.Length; i++)
        {
            Transform stone = stones[i];
            stone.localEulerAngles = Vector3.zero;
            Vector3 localPos = stone.localPosition;
            _sequence.Insert(0, stone.DOLocalRotate(new Vector3(0f, 0f, 360f * 2f), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutElastic)
                .OnComplete(
                    () =>
                    {
                        stone.localEulerAngles = Vector3.zero;
                        stone.localPosition = localPos;
                    }));
            float x = 3f;
            _sequence.Insert(duration / x, stone.DOLocalMove(localPos * 2f, duration / x).SetEase(Ease.InOutSine));
            _sequence.Insert(duration - duration / x, stone.DOLocalMove(localPos, duration / x).SetEase(Ease.InOutSine));

        }

        _sequence.SetLoops(-1);
    }

    public void OnDisable()
    {
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
    
    public void OnDestroy()
    {
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }

}
