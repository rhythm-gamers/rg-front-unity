using UnityEngine;

public class SlideAnimation : MonoBehaviour
{
    public RectTransform canvas;
    public RectTransform leftPanel;
    public RectTransform rightPanel;

    private int currentIdx = 0;

    void OnEnable()
    {
        currentIdx = 0;
        leftPanel.anchoredPosition = Vector2.zero;
        rightPanel.anchoredPosition = new Vector2(canvas.rect.width, 0);
    }

    public void SlideToRight()
    {
        if (currentIdx == 1) return;
        currentIdx = 1;
        StartCoroutine(AniPreset.Instance.IEAniMoveToTarget(rightPanel, leftPanel, 4f));
    }

    public void SlideToLeft()
    {
        if (currentIdx == 0) return;
        currentIdx = 0;
        StartCoroutine(AniPreset.Instance.IEAniMoveToTarget(leftPanel, rightPanel, 4f));
    }
}