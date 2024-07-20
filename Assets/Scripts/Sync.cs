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

    public GameObject judgeLine;

    [DllImport("__Internal")]
    private static extern void SetJudgeTime(int judgeTime);

    SpriteRenderer sr;
    UIText uiSyncTime;

    Coroutine coPopup;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        judgeLine.transform.localPosition = new Vector3(0f, DiffFromNoteBtm, 0f); // 노트 높이의 절반만큼 판정선을 올림 (에디터가 노트 바닥을 기준으로 스냅을 잡기 때문)
        sr = judgeLine.GetComponent<SpriteRenderer>();
        sr.color = Color.red;
    }

    public void Down()
    {
        Judgement.Instance.judgeTimeFromUserSetting -= 25;
        uiSyncTime = UIController.Instance.FindUI("UI_G_SyncTime").uiObject as UIText;

        int time = Mathf.Abs(Judgement.Instance.judgeTimeFromUserSetting);
        string txt = $"{time} ms";
        if (Judgement.Instance.judgeTimeFromUserSetting < 0)
            txt = $"{time} ms SLOW";
        else if (Judgement.Instance.judgeTimeFromUserSetting > 0)
            txt = $"{time} ms FAST";

        uiSyncTime.SetText(txt);
        uiSyncTime.GetComponent<RectTransform>().anchoredPosition3D += Vector3.down * 2.5f;

#if UNITY_WEBGL == true && UNITY_EDITOR == false
            SetJudgeTime(judgement.judgeTimeFromUserSetting);
#endif

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(AniPreset.Instance.IETextPopup(uiSyncTime, 1f));
    }

    public void Up()
    {
        Judgement.Instance.judgeTimeFromUserSetting += 25;
        uiSyncTime = UIController.Instance.FindUI("UI_G_SyncTime").uiObject as UIText;

        int time = Mathf.Abs(Judgement.Instance.judgeTimeFromUserSetting);
        string txt = $"{time} ms";
        if (Judgement.Instance.judgeTimeFromUserSetting < 0)
            txt = $"{time} ms SLOW";
        else if (Judgement.Instance.judgeTimeFromUserSetting > 0)
            txt = $"{time} ms FAST";

        uiSyncTime.SetText(txt);
        uiSyncTime.GetComponent<RectTransform>().anchoredPosition3D += Vector3.up * 2.5f;

#if UNITY_WEBGL == true && UNITY_EDITOR == false
            SetJudgeTime(judgement.judgeTimeFromUserSetting);
#endif

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(AniPreset.Instance.IETextPopup(uiSyncTime, 1f));
    }
}
