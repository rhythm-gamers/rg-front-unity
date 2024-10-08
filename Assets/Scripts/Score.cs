using System;
using UnityEngine;

public struct JudgeCategory
{
    public int Long;
    public int Short;

    // Fast 및 Slow는 Rhythm에는 적용되지 않음
    public int Fast;
    public int Slow;
    public int Total { get { return Long + Short; } }
}

public struct ScoreData
{
    public JudgeCategory rhythm; // perfect
    public JudgeCategory great;
    public JudgeCategory good;
    public JudgeCategory miss;

    public string[] judgeText;
    public Color[] judgeColor;
    public JudgeType judge;
    public int combo;
    public int Score
    {
        get
        {
            return (rhythm.Total * 1000) + (great.Total * 500) + (good.Total * 200);
        }
        set
        {
            Score = value;
        }
    }
}

public class Score : MonoBehaviour
{
    static Score instance;
    public static Score Instance
    {
        get { return instance; }
    }

    public ScoreData data;

    UIText uiJudgement;
    UIText uiCombo;
    UIText uiScore;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        uiJudgement = UIController.Instance.FindUI("UI_G_Judgement").uiObject as UIText;
        uiCombo = UIController.Instance.FindUI("UI_G_Combo").uiObject as UIText;
        uiScore = UIController.Instance.FindUI("UI_G_Score").uiObject as UIText;

        AniPreset.Instance.Join(uiJudgement.Name);
        AniPreset.Instance.Join(uiCombo.Name);
        AniPreset.Instance.Join(uiScore.Name);
    }

    public void Clear()
    {
        data = new ScoreData
        {
            judgeText = Enum.GetNames(typeof(JudgeType)),
            judgeColor = new Color[4] {
                new Color(24 / 255f, 116 / 255f, 255 / 255f), // rhythm
                new Color(54 / 255f, 255 / 255f, 124 / 255f), // great
                new Color(249 / 255f, 217 / 255f, 35 / 255f), // good
                new Color(255 / 255f, 83 / 255f, 83 / 255f)  // miss
            }
        };
        uiJudgement.SetText("");
        uiCombo.SetText("");
        uiScore.SetText("0");
    }

    public void UpdateScore()
    {
        uiJudgement.SetText(data.judgeText[(int)data.judge]);
        uiJudgement.SetColor(data.judgeColor[(int)data.judge]);
        uiCombo.SetText($"{data.combo}");
        uiScore.SetText($"{data.Score}");

        AniPreset.Instance.PlayPop(uiJudgement.Name, uiJudgement.rect);
        AniPreset.Instance.PlayPop(uiCombo.Name, uiCombo.rect);
        //UIController.Instance.find.Invoke(uiJudgement.Name);
        //UIController.Instance.find.Invoke(uiCombo.Name);
    }

    public void Ani(UIObject uiObject)
    {
        //AniPreset.Instance.PlayPop(uiObject.Name, uiObject.rect);
    }
}