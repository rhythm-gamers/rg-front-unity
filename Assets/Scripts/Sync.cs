using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class Sync : MonoBehaviour
{
    static Sync instance;
    public static Sync Instance
    {
        get
        {
            return instance;
        }
    }

    public float DiffFromNoteBtm { get; set; }

    public GameObject judgeObjects;

    UIText uiSyncTime;

    Coroutine coPopup;

    private int judgeOffsetFromUser = 0;
    IEnumerator WebGLInitUserJudgeOffset(int judgeOffset)
    {
        judgeOffsetFromUser = judgeOffset;
        yield return new WaitUntil(() => UIController.Instance.isInit == true);
    }

    [DllImport("__Internal")]
    private static extern void SetJudgeOffset(int judgeOffset);

    [DllImport("__Internal")]
    private static extern void SetSpeed(string speed);

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        GameObject judgeLine = judgeObjects.transform.Find("JudgeLine").gameObject;
        judgeLine.transform.localPosition += Vector3.up * DiffFromNoteBtm; // 노트 높이의 절반만큼 판정선을 올림 (에디터가 노트 바닥을 기준으로 스냅을 잡기 때문)
    }

    public void SpeedDown()
    {
        GameManager.Instance.Speed -= 0.1f;
        NoteGenerator.Instance.Interpolate();

        string speedToString = GameManager.Instance.Speed.ToString("0.0");
        UIText inGameSpeedUI = UIController.Instance.FindUI("UI_G_Speed").uiObject as UIText;
        UIText outGameSpeedUI = UIController.Instance.FindUI("UI_D_Speed").uiObject as UIText;
        inGameSpeedUI.SetText("Speed " + speedToString);
        outGameSpeedUI.SetText("Speed " + speedToString);
#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
    }

    public void SpeedUp()
    {
        GameManager.Instance.Speed += 0.1f;
        NoteGenerator.Instance.Interpolate();

        string speedToString = GameManager.Instance.Speed.ToString("0.0");
        UIText inGameSpeedUI = UIController.Instance.FindUI("UI_G_Speed").uiObject as UIText;
        UIText outGameSpeedUI = UIController.Instance.FindUI("UI_D_Speed").uiObject as UIText;
        inGameSpeedUI.SetText("Speed " + speedToString);
        outGameSpeedUI.SetText("Speed " + speedToString);
#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
    }

    public void JudgeOffsetDown()
    {
        if (judgeOffsetFromUser - 1 < -100) return;

        judgeOffsetFromUser -= 1;
        uiSyncTime = UIController.Instance.FindUI("UI_G_SyncTime").uiObject as UIText;

        float distance = 0.01f;
        judgeObjects.transform.localPosition += Vector3.down * distance;

        int offset = Mathf.Abs(judgeOffsetFromUser);
        string txt = $"{offset} offset";
        if (judgeOffsetFromUser < 0)
            txt = $"-{offset} offset";
        else if (judgeOffsetFromUser > 0)
            txt = $"{offset} offset";

        uiSyncTime.SetText(txt);
        uiSyncTime.GetComponent<RectTransform>().anchoredPosition3D += Vector3.down;

#if UNITY_WEBGL && !UNITY_EDITOR
        SetJudgeOffset(judgeOffsetFromUser);
#endif

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(AniPreset.Instance.IETextPopup(uiSyncTime, 1f));
    }

    public void JudgeOffsetUp()
    {
        if (judgeOffsetFromUser + 1 > 250) return;

        judgeOffsetFromUser += 1;
        uiSyncTime = UIController.Instance.FindUI("UI_G_SyncTime").uiObject as UIText;

        float distance = 0.01f;
        judgeObjects.transform.localPosition += Vector3.up * distance;

        int offset = Mathf.Abs(judgeOffsetFromUser);
        string txt = $"{offset} offset";
        if (judgeOffsetFromUser < 0)
            txt = $"-{offset} offset";
        else if (judgeOffsetFromUser > 0)
            txt = $"{offset} offset";

        uiSyncTime.SetText(txt);
        uiSyncTime.GetComponent<RectTransform>().anchoredPosition3D += Vector3.up;

#if UNITY_WEBGL && !UNITY_EDITOR
        SetJudgeOffset(judgeOffsetFromUser);
#endif

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(AniPreset.Instance.IETextPopup(uiSyncTime, 1f));
    }
}
