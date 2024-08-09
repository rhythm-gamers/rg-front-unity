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

    public GameObject notePrefab;

    public GameObject judgeObjects;

    UIText syncTimeUI;
    UIText inGameSpeedUI;
    UIText outGameSpeedUI;
    UIText offsetUI;

    Coroutine coPopup;

    private int judgeOffsetFromUser = 0;
    private bool isJudgeOffsetInit = false;
    void WebGLInitUserJudgeOffset(int judgeOffset)
    {
        judgeOffsetFromUser = judgeOffset;
        isJudgeOffsetInit = true;
    }

    [DllImport("__Internal")]
    private static extern void SetJudgeOffset(int judgeOffset);

    [DllImport("__Internal")]
    private static extern void SetSpeed(string speed);

    private readonly float offsetInterval = 0.01f;

    void Awake()
    {
        if (instance == null)
            instance = this;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        isJudgeOffsetInit = true;
#endif
    }

    public void Init()
    {
        StartCoroutine(IEInit());
    }

    private IEnumerator IEInit()
    {
        yield return new WaitUntil(() => isJudgeOffsetInit && UIController.Instance.isInit);

        syncTimeUI = UIController.Instance.GetUI("UI_G_SyncTime").uiObject as UIText;
        inGameSpeedUI = UIController.Instance.GetUI("UI_G_Speed").uiObject as UIText;
        outGameSpeedUI = UIController.Instance.GetUI("UI_D_Speed").uiObject as UIText;
        offsetUI = UIController.Instance.GetUI("UI_D_Offset").uiObject as UIText;

        GameObject note = Instantiate(notePrefab);
        float DiffFromNoteBtm = note.GetComponent<SpriteRenderer>().bounds.center.y;

        GameObject judgeLine = judgeObjects.transform.Find("JudgeLine").gameObject;
        judgeObjects.transform.localPosition = offsetInterval * judgeOffsetFromUser * Vector3.up;
        judgeLine.transform.localPosition = Vector3.up * DiffFromNoteBtm; // 노트 높이의 절반만큼 판정선을 올림 (에디터가 노트 바닥을 기준으로 스냅을 잡기 때문)

        offsetUI.SetText($"Offset\n{judgeOffsetFromUser}");
        syncTimeUI.GetComponent<RectTransform>().anchoredPosition3D += Vector3.up * DiffFromNoteBtm;

        Destroy(note);
    }

    public void SpeedDown()
    {
        GameManager.Instance.Speed -= 0.1f;
        NoteGenerator.Instance.Interpolate();

        string speedToString = GameManager.Instance.Speed.ToString("0.0");
        inGameSpeedUI.SetText($"Speed\n{speedToString}");
        outGameSpeedUI.SetText($"Speed\n{speedToString}");
#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
    }

    public void SpeedUp()
    {
        GameManager.Instance.Speed += 0.1f;
        NoteGenerator.Instance.Interpolate();

        string speedToString = GameManager.Instance.Speed.ToString("0.0");
        inGameSpeedUI.SetText($"Speed\n{speedToString}");
        outGameSpeedUI.SetText($"Speed\n{speedToString}");
#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
    }

    public void JudgeOffsetDown()
    {
        if (judgeOffsetFromUser - 1 < -100) return;

        judgeOffsetFromUser -= 1;
        judgeObjects.transform.localPosition += Vector3.down * offsetInterval;

        int offset = Mathf.Abs(judgeOffsetFromUser);
        string txt = $"{offset} offset";
        if (judgeOffsetFromUser < 0)
            txt = $"-{offset} offset";
        else if (judgeOffsetFromUser > 0)
            txt = $"{offset} offset";

        syncTimeUI.SetText(txt);
        syncTimeUI.GetComponent<RectTransform>().anchoredPosition3D += Vector3.down;

        offsetUI.SetText($"Offset\n{judgeOffsetFromUser}");

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(AniPreset.Instance.IETextPopup(syncTimeUI, 1f));

#if UNITY_WEBGL && !UNITY_EDITOR
        SetJudgeOffset(judgeOffsetFromUser);
#endif
    }

    public void JudgeOffsetUp()
    {
        if (judgeOffsetFromUser + 1 > 250) return;

        judgeOffsetFromUser += 1;
        judgeObjects.transform.localPosition += Vector3.up * offsetInterval;

        int offset = Mathf.Abs(judgeOffsetFromUser);
        string txt = $"{offset} offset";
        if (judgeOffsetFromUser < 0)
            txt = $"-{offset} offset";
        else if (judgeOffsetFromUser > 0)
            txt = $"{offset} offset";

        syncTimeUI.SetText(txt);
        syncTimeUI.GetComponent<RectTransform>().anchoredPosition3D += Vector3.up;

        offsetUI.SetText($"Offset\n{judgeOffsetFromUser}");

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(AniPreset.Instance.IETextPopup(syncTimeUI, 1f));

#if UNITY_WEBGL && !UNITY_EDITOR
        SetJudgeOffset(judgeOffsetFromUser);
#endif
    }

#if !UNITY_WEBGL
    public void ResetJudgeOffset()
    {
        syncTimeUI.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(450, -430, 0); // 초기 위치

        judgeOffsetFromUser = 0;
        offsetUI.SetText("Offset\n0");
        judgeObjects.transform.localPosition = Vector3.zero;
    }
#endif
}
