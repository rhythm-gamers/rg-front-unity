#if !UNITY_WEBGL

using System;
using System.Collections;
using UnityEngine;

public class Editor : MonoBehaviour
{
    static Editor instance;
    public static Editor Instance
    {
        get
        {
            return instance;
        }
    }

    UISlider slider = null;
    UIButton musicController = null;
    UIText timer = null;

    public GameObject objects;

    Coroutine coMove;
    Coroutine coPopup;

    public int currentBar = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public float speed;
    public void Init()
    {
        GridGenerator gridGenerator = FindObjectOfType<GridGenerator>();
        speed = gridGenerator.barInterval / GameManager.Instance.sheet.BarPerSec;

        slider = UIController.Instance.GetUI("UI_E_ProgressBar").uiObject as UISlider;
        musicController = UIController.Instance.GetUI("UI_E_Play").uiObject as UIButton;
        timer = UIController.Instance.GetUI("UI_E_Time").uiObject as UIText;

        StartCoroutine(IEBarTimer());

        objects.transform.position = Vector3.zero;
    }

    void Update()
    {
        if (slider != null)
        {
            float value = Mathf.Clamp(1 / AudioManager.Instance.Length * AudioManager.Instance.progressTime, 0f, 1f);
            slider.slider.value = value;
        }
        if (timer != null)
        {
            timer.SetText(TimeSpan.FromSeconds(AudioManager.Instance.progressTime).ToString(@"mm\:ss\:fff"));
        }
    }

    public void Move()
    {
        if (coMove != null)
            StopCoroutine(coMove);

        coMove = StartCoroutine(IEMove());
    }

    public void PlayOrPause()
    {
        if (AudioManager.Instance.IsPlaying())
        {
            AudioManager.Instance.Pause();
            musicController.SetText(">");
            if (coMove != null)
                StopCoroutine(coMove);
        }
        else
        {
            AudioManager.Instance.UnPause();
            musicController.SetText("||");
            coMove = StartCoroutine(IEMove());
        }
    }

    public void Stop()
    {
        if (coMove != null)
            StopCoroutine(coMove);

        objects.transform.position = Vector3.zero;

        AudioManager.Instance.Pause();
        AudioManager.Instance.progressTime = 0f;

        musicController.SetText(">");
    }

    public void CalculateCurrentBar()
    {
        currentBar = (int)(AudioManager.Instance.progressTime * 1000 / GameManager.Instance.sheet.BarPerMilliSec);
    }

    IEnumerator IEBarTimer()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            CalculateCurrentBar();
            yield return wait;
        }
    }



    public IEnumerator IEMove()
    {
        while (true)
        {
            if (GameManager.Instance.state == GameManager.GameState.Game)
                objects.transform.position += Vector3.down * Time.deltaTime * speed * GameManager.Instance.Speed;
            else
                objects.transform.position += Vector3.down * Time.deltaTime * speed;
            yield return null;
        }
    }

    public void Play(UIObject uiObject)
    {
        PlayOrPause();
    }

    public void Stop(UIObject uiObject)
    {
        Stop();
    }

    public void CalibratePosition()
    {
        if (slider != null)
        {
            float time = AudioManager.Instance.Length * slider.slider.value;
            AudioManager.Instance.progressTime = time;

            // 음악 타임에 맞춰서 오브젝트스 이동
            // 한마디에 16씩 이동
            // time / 한마디 시간

            CalculateCurrentBar();

            // 한 그리드(한 마디)의 게임오브젝트 y좌표의 높이는 16
            // 현재 음악위치 * 16 = 높이s
            float barPerTime = GameManager.Instance.sheet.BarPerSec;
            float pos = time / barPerTime * 16;

            objects.transform.position = new Vector3(0f, -pos, 0f);
        }
    }

    public void SelectShortNote()
    {
        if (EditorController.Instance.isLongNoteActive)
            EditorController.Instance.isLongNoteActive = false;

        bool nextState = !EditorController.Instance.isShortNoteActive;

        EditorController.Instance.isShortNoteActive = nextState;
        EditorController.Instance.SetActiveCursor(nextState);
    }

    public void SelectLongNote()
    {
        if (EditorController.Instance.isShortNoteActive)
            EditorController.Instance.isShortNoteActive = false;

        bool nextState = !EditorController.Instance.isLongNoteActive;

        EditorController.Instance.isLongNoteActive = nextState;
        EditorController.Instance.SetActiveCursor(nextState);
    }



    public void SheetSave()
    {
        SheetStorage.Instance.SaveEditedSheet();
    }
    public void SheetUpload()
    {
        SheetStorage.Instance.Upload();
    }
    public void SheetDownload()
    {
        SheetStorage.Instance.Download();
    }

    public void ShowProgressLog(string log)
    {
        if (coPopup != null)
            StopCoroutine(coPopup);

        UIText uiProgressLog = UIController.Instance.FindUI("UI_E_ProgressLog").uiObject as UIText;
        uiProgressLog.SetText(log);

        coPopup = StartCoroutine(AniPreset.Instance.IETextPopup(uiProgressLog, 3f));
    }
}

#endif