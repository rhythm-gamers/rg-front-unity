using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class Sync : MonoBehaviour
{
    Judgement judgement;

    public GameObject judgeLine;

    [DllImport("__Internal")]
    private static extern void SetJudgeTime(int judgeTime);

    SpriteRenderer sr;
    UIText text;

    Coroutine coPopup;

    // Start is called before the first frame update
    void Start()
    {
        judgement = FindObjectOfType<Judgement>();
        sr = judgeLine.GetComponent<SpriteRenderer>();
        sr.color = Color.red;
    }

    public void Down()
    {
        judgement.judgeTimeFromUserSetting -= 25;
        text = UIController.Instance.FindUI("UI_G_SyncTime").uiObject as UIText;

        int time = Mathf.Abs(judgement.judgeTimeFromUserSetting);
        string txt = $"{time} ms";
        if (judgement.judgeTimeFromUserSetting < 0)
            txt = $"{time} ms SLOW";
        else if (judgement.judgeTimeFromUserSetting > 0)
            txt = $"{time} ms FAST";

        text.SetText(txt);
        text.GetComponent<RectTransform>().anchoredPosition3D += Vector3.down * 2.5f;

#if UNITY_WEBGL == true && UNITY_EDITOR == false
            SetJudgeTime(judgement.judgeTimeFromUserSetting);
#endif

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(IEPopup());
    }

    public void Up()
    {
        judgement.judgeTimeFromUserSetting += 25;
        text = UIController.Instance.FindUI("UI_G_SyncTime").uiObject as UIText;

        int time = Mathf.Abs(judgement.judgeTimeFromUserSetting);
        string txt = $"{time} ms";
        if (judgement.judgeTimeFromUserSetting < 0)
            txt = $"{time} ms SLOW";
        else if (judgement.judgeTimeFromUserSetting > 0)
            txt = $"{time} ms FAST";

        text.SetText(txt);
        text.GetComponent<RectTransform>().anchoredPosition3D += Vector3.up * 2.5f;

#if UNITY_WEBGL == true && UNITY_EDITOR == false
            SetJudgeTime(judgement.judgeTimeFromUserSetting);
#endif

        if (coPopup != null)
            StopCoroutine(coPopup);
        coPopup = StartCoroutine(IEPopup());
    }

    IEnumerator IEPopup()
    {
        text.SetColor(sr.color);
        float time = 0f;
        float speed = 4f;
        while (time < 1f)
        {
            text.SetColor(new Color(1, 1, 1, time));

            time += Time.deltaTime * speed;
            yield return null;
        }
        text.SetColor(Color.white);
        yield return new WaitForSeconds(1f);

        time = 0f;
        while (time < 1f)
        {
            text.SetColor(new Color(1, 1, 1, 1 - time));

            time += Time.deltaTime * speed;
            yield return null;
        }
        text.SetColor(new Color(1, 1, 1, 0));
    }
}
