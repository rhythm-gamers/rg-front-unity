using UnityEngine;

public class GameResultController : MonoBehaviour
{
    static GameResultController instance;
    public static GameResultController Instance
    {
        get
        {
            return instance;
        }
    }

    public Transform DeviationPanel;
    public GameObject DeviationPointPrefab;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        InitFirstPage();
        InitSecondPage();
    }

    private void InitFirstPage()
    {
        UIText ScoreUI = UIController.Instance.FindUI("UI_R_Score").uiObject as UIText;
        UIText RhythmUI = UIController.Instance.FindUI("UI_R_Rhythm").uiObject as UIText;
        UIText GreatUI = UIController.Instance.FindUI("UI_R_Great").uiObject as UIText;
        UIText GoodUI = UIController.Instance.FindUI("UI_R_Good").uiObject as UIText;
        UIText FastMissUI = UIController.Instance.FindUI("UI_R_FastMiss").uiObject as UIText;
        UIText TotalMissUI = UIController.Instance.FindUI("UI_R_TotalMiss").uiObject as UIText;
        UIText SlowMissUI = UIController.Instance.FindUI("UI_R_SlowMiss").uiObject as UIText;

        ScoreUI.SetText(Score.Instance.data.Score.ToString());
        RhythmUI.SetText(Score.Instance.data.rhythm.Total.ToString());
        GreatUI.SetText(Score.Instance.data.great.Total.ToString());
        GoodUI.SetText(Score.Instance.data.good.Total.ToString());
        FastMissUI.SetText(Score.Instance.data.miss.Fast.ToString());
        TotalMissUI.SetText(Score.Instance.data.miss.Total.ToString());
        SlowMissUI.SetText(Score.Instance.data.miss.Slow.ToString());

        UIImage rBG = UIController.Instance.FindUI("UI_R_BG").uiObject as UIImage;
        rBG.SetSprite(GameManager.Instance.sheet.img);
    }

    private void InitSecondPage()
    {
        int judgedNoteLength = Judgement.Instance.GetJudgedNoteLength();
        Debug.Log($"총 인식된 노트수: {judgedNoteLength}");

        InitJudgeGrid();
        InitJudgeGraph();
        InitJudgeTimelineGraph();
    }

    private void InitJudgeGrid()
    {
        UIText LongRhythmUI = UIController.Instance.FindUI("UI_RA_LongRhythm").uiObject as UIText;
        UIText ShortRhythmUI = UIController.Instance.FindUI("UI_RA_ShortRhythm").uiObject as UIText;
        UIText TotalRhythmUI = UIController.Instance.FindUI("UI_RA_TotalRhythm").uiObject as UIText;
        UIText LongGreatUI = UIController.Instance.FindUI("UI_RA_LongGreat").uiObject as UIText;
        UIText ShortGreatUI = UIController.Instance.FindUI("UI_RA_ShortGreat").uiObject as UIText;
        UIText TotalGreatUI = UIController.Instance.FindUI("UI_RA_TotalGreat").uiObject as UIText;
        UIText LongGoodUI = UIController.Instance.FindUI("UI_RA_LongGood").uiObject as UIText;
        UIText ShortGoodUI = UIController.Instance.FindUI("UI_RA_ShortGood").uiObject as UIText;
        UIText TotalGoodUI = UIController.Instance.FindUI("UI_RA_TotalGood").uiObject as UIText;
        UIText LongMissUI = UIController.Instance.FindUI("UI_RA_LongMiss").uiObject as UIText;
        UIText ShortMissUI = UIController.Instance.FindUI("UI_RA_ShortMiss").uiObject as UIText;
        UIText TotalMissUI = UIController.Instance.FindUI("UI_RA_TotalMiss").uiObject as UIText;

        LongRhythmUI.SetText(Score.Instance.data.rhythm.Long.ToString());
        ShortRhythmUI.SetText(Score.Instance.data.rhythm.Short.ToString());
        TotalRhythmUI.SetText(Score.Instance.data.rhythm.Total.ToString());
        LongGreatUI.SetText(Score.Instance.data.great.Long.ToString());
        ShortGreatUI.SetText(Score.Instance.data.great.Short.ToString());
        TotalGreatUI.SetText(Score.Instance.data.great.Total.ToString());
        LongGoodUI.SetText(Score.Instance.data.good.Long.ToString());
        ShortGoodUI.SetText(Score.Instance.data.good.Short.ToString());
        TotalGoodUI.SetText(Score.Instance.data.good.Total.ToString());
        LongMissUI.SetText(Score.Instance.data.miss.Long.ToString());
        ShortMissUI.SetText(Score.Instance.data.miss.Short.ToString());
        TotalMissUI.SetText(Score.Instance.data.miss.Total.ToString());
    }

    private void InitJudgeGraph()
    {
        UISlider RhythmSlider = UIController.Instance.FindUI("UI_RA_Slider_Rhythm").uiObject as UISlider;
        UISlider FastGreatSlider = UIController.Instance.FindUI("UI_RA_Slider_FastGreat").uiObject as UISlider;
        UISlider SlowGreatSlider = UIController.Instance.FindUI("UI_RA_Slider_SlowGreat").uiObject as UISlider;
        UISlider FastGoodSlider = UIController.Instance.FindUI("UI_RA_Slider_FastGood").uiObject as UISlider;
        UISlider SlowGoodSlider = UIController.Instance.FindUI("UI_RA_Slider_SlowGood").uiObject as UISlider;
        UISlider FastMissSlider = UIController.Instance.FindUI("UI_RA_Slider_FastMiss").uiObject as UISlider;
        UISlider SlowMissSlider = UIController.Instance.FindUI("UI_RA_Slider_SlowMiss").uiObject as UISlider;

        UIText RhythmUI = UIController.Instance.FindUI("UI_RA_Rhythm").uiObject as UIText;
        UIText FastGreatUI = UIController.Instance.FindUI("UI_RA_FastGreat").uiObject as UIText;
        UIText SlowGreatUI = UIController.Instance.FindUI("UI_RA_SlowGreat").uiObject as UIText;
        UIText FastGoodUI = UIController.Instance.FindUI("UI_RA_FastGood").uiObject as UIText;
        UIText SlowGoodUI = UIController.Instance.FindUI("UI_RA_SlowGood").uiObject as UIText;
        UIText FastMissUI = UIController.Instance.FindUI("UI_RA_FastMiss").uiObject as UIText;
        UIText SlowMissUI = UIController.Instance.FindUI("UI_RA_SlowMiss").uiObject as UIText;

        int judgedNoteLength = Judgement.Instance.GetJudgedNoteLength();

        RhythmSlider.slider.value = GetSlideValue(Score.Instance.data.rhythm.Total, judgedNoteLength);
        FastGreatSlider.slider.value = GetSlideValue(Score.Instance.data.great.Fast, judgedNoteLength);
        SlowGreatSlider.slider.value = GetSlideValue(Score.Instance.data.great.Slow, judgedNoteLength);
        FastGoodSlider.slider.value = GetSlideValue(Score.Instance.data.good.Fast, judgedNoteLength);
        SlowGoodSlider.slider.value = GetSlideValue(Score.Instance.data.good.Slow, judgedNoteLength);
        FastMissSlider.slider.value = GetSlideValue(Score.Instance.data.miss.Fast, judgedNoteLength);
        SlowMissSlider.slider.value = GetSlideValue(Score.Instance.data.miss.Slow, judgedNoteLength);

        RhythmUI.SetText(Score.Instance.data.rhythm.Total.ToString());
        FastGreatUI.SetText(Score.Instance.data.great.Fast.ToString());
        SlowGreatUI.SetText(Score.Instance.data.great.Slow.ToString());
        FastGoodUI.SetText(Score.Instance.data.good.Fast.ToString());
        SlowGoodUI.SetText(Score.Instance.data.good.Slow.ToString());
        FastMissUI.SetText(Score.Instance.data.miss.Fast.ToString());
        SlowMissUI.SetText(Score.Instance.data.miss.Slow.ToString());
    }

    private void InitJudgeTimelineGraph()
    {
        UIText PredictionIntervalUI = UIController.Instance.FindUI("UI_RA_PredictionInterval").uiObject as UIText;
        int judgedNoteLength = Judgement.Instance.GetJudgedNoteLength();

        foreach (Transform child in DeviationPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < judgedNoteLength; i++)
        {
            GameObject point = Instantiate(DeviationPointPrefab, DeviationPanel);
            int inputTime = Judgement.Instance.GetInputTimeAt(i);
            int judgeTime = Judgement.Instance.GetJudgeTimeAt(i);
            point.transform.localPosition = new Vector3(inputTime * 1200f / (AudioManager.Instance.Length * 1000f), judgeTime * 200f
             / 600f);
        }

        PredictionIntervalUI.SetText($"예측 판정 범위: {Judgement.Instance.Average:F0}ms ±{Judgement.Instance.PredictionInterval:F0}ms");

    }

    private float GetSlideValue(float value, float noteLength)
    {
        if (noteLength == 0) return 0f;
        return value / noteLength;
    }
}
