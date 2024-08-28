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

    /// <summary>
    /// User에 의해 조정된 판정 타이밍
    /// </summary>
    public int judgeOffsetFromUser = 0;
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

        UIText inGameOffsetUI = UIController.Instance.GetUI("UI_G_JudgeOffset").uiObject as UIText;
        UIText outGameOffsetUI = UIController.Instance.GetUI("UI_D_JudgeOffset").uiObject as UIText;

        GameObject note = Instantiate(notePrefab);
        float DiffFromNoteBtm = note.GetComponent<SpriteRenderer>().bounds.center.y;

        GameObject judgeLine = judgeObjects.transform.Find("JudgeLine").gameObject;
        judgeLine.transform.localPosition = Vector3.up * DiffFromNoteBtm; // 노트 높이의 절반만큼 판정선을 올림 (에디터가 노트 바닥을 기준으로 스냅을 잡기 때문)

        inGameOffsetUI.SetText($"{judgeOffsetFromUser}ms");
        outGameOffsetUI.SetText($"{judgeOffsetFromUser}ms");

        Destroy(note);
    }

    public void SpeedDown()
    {
        UIText inGameSpeedUI = UIController.Instance.GetUI("UI_G_Speed").uiObject as UIText;
        UIText outGameSpeedUI = UIController.Instance.GetUI("UI_D_Speed").uiObject as UIText;

        GameManager.Instance.Speed -= 0.1f;

        string speedToString = GameManager.Instance.Speed.ToString("0.0");
        inGameSpeedUI.SetText(speedToString);
        outGameSpeedUI.SetText(speedToString);

        if (GameManager.Instance.isPlaying)
            NoteGenerator.Instance.Interpolate();

#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
    }

    public void SpeedUp()
    {
        UIText inGameSpeedUI = UIController.Instance.GetUI("UI_G_Speed").uiObject as UIText;
        UIText outGameSpeedUI = UIController.Instance.GetUI("UI_D_Speed").uiObject as UIText;

        GameManager.Instance.Speed += 0.1f;

        string speedToString = GameManager.Instance.Speed.ToString("0.0");
        inGameSpeedUI.SetText(speedToString);
        outGameSpeedUI.SetText(speedToString);

        if (GameManager.Instance.isPlaying)
            NoteGenerator.Instance.Interpolate();

#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
    }

    public void JudgeOffsetDown()
    {
        UIText inGameOffsetUI = UIController.Instance.GetUI("UI_G_JudgeOffset").uiObject as UIText;
        UIText outGameOffsetUI = UIController.Instance.GetUI("UI_D_JudgeOffset").uiObject as UIText;
        
        if (judgeOffsetFromUser - 5 < -200) return;
        judgeOffsetFromUser -= 5;
        NoteGenerator.Instance.Interpolate();

        inGameOffsetUI.SetText($"{judgeOffsetFromUser}ms");
        outGameOffsetUI.SetText($"{judgeOffsetFromUser}ms");

#if UNITY_WEBGL && !UNITY_EDITOR
        SetJudgeOffset(judgeOffsetFromUser);
#endif
    }

    public void JudgeOffsetUp()
    {
        UIText inGameOffsetUI = UIController.Instance.GetUI("UI_G_JudgeOffset").uiObject as UIText;
        UIText outGameOffsetUI = UIController.Instance.GetUI("UI_D_JudgeOffset").uiObject as UIText;
        
        if (judgeOffsetFromUser + 5 > 200) return;
        judgeOffsetFromUser += 5;
        NoteGenerator.Instance.Interpolate();

        inGameOffsetUI.SetText($"{judgeOffsetFromUser}ms");
        outGameOffsetUI.SetText($"{judgeOffsetFromUser}ms");

#if UNITY_WEBGL && !UNITY_EDITOR
        SetJudgeOffset(judgeOffsetFromUser);
#endif
    }
}
