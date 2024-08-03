using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPanel : Singleton<ResultPanel>
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI whitePoints;
    [SerializeField] private TextMeshProUGUI blackPoints;
    [SerializeField] private Transform whiteCrown;
    [SerializeField] private Transform blackCrown;

    public override void Awake()
    {
        base.Awake();
    }


    public void InitializeResult()
    {
        UIMgr.Instance.ActiveElement("Result Panel");
        Vector2Int result = GameMgr.Instance.currentGameboard.CalculatePoints();
        whiteCrown.localScale = Vector3.zero;
        blackCrown.localScale = Vector3.zero;
        bool whiteWin = result.x + 6.5f > result.y;
        title.text = (whiteWin ? "White" : "Black") + " Wins!!!";
        float d = 2f;
        DOTween.To(() => 0, x =>
            {
                whitePoints.text = $"{x} <size=60%>+6.5</size>";
                whitePoints.transform.DOKill();
                whitePoints.transform.localScale = Vector3.one;
                whitePoints.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 1);
            }, result.x, result.x / 10f)
            .OnComplete(() =>
            {
                whitePoints.text = $"{result.x} <size=45%>+6.5</size>";
                if (whiteWin)
                {
                    whiteCrown.DOKill();
                    whiteCrown.DOScale(1f, 0.5f).SetEase(Ease.OutElastic);
                }
            });

        
        DOTween.To(() => 0, x =>
        {
            blackPoints.text = x.ToString();
            blackPoints.transform.DOKill();
            blackPoints.transform.localScale = Vector3.one;
            blackPoints.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 1);
        }, result.y, result.y / 10f).OnComplete(() =>
        {
            blackPoints.text = $"{result.y}";
            if (!whiteWin)
            {
                blackCrown.DOKill();
                blackCrown.DOScale(1f, 0.5f).SetEase(Ease.OutElastic);
            }
        });

    }
}
