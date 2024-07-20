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

    int snap = 4;
    public int Snap
    {
        get { return snap; }
        set
        {
            snap = Mathf.Clamp(value, 1, 16);
        }
    }

    public int currentBar = 0;
    public float offsetPosition;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    float speed;
    public void Init()
    {
        slider = UIController.Instance.GetUI("UI_E_ProgressBar").uiObject as UISlider;
        musicController = UIController.Instance.GetUI("UI_E_Play").uiObject as UIButton;
        timer = UIController.Instance.GetUI("UI_E_Time").uiObject as UIText;

        StartCoroutine(IEBarTimer());

        speed = 16 / GameManager.Instance.editorSheet.BarPerSec;
        offsetPosition = speed * GameManager.Instance.editorSheet.offset * 0.001f;
        objects.transform.position = offsetPosition * Vector3.up;
    }

    void Update()
    {
        float value = Mathf.Clamp(1 / AudioManager.Instance.Length * AudioManager.Instance.progressTime, 0f, 1f);
        if (slider != null)
        {
            slider.slider.value = value;
        }
        if (timer != null)
        {
            timer.SetText(TimeSpan.FromSeconds(AudioManager.Instance.progressTime).ToString(@"mm\:ss\:fff"));
        }
    }

    public void Play()
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

        objects.transform.position = new Vector3(0f, offsetPosition, 0f);

        AudioManager.Instance.Pause();
        AudioManager.Instance.progressTime = 0f;

        musicController.SetText(">");
    }

    public void CalculateCurrentBar()
    {
        currentBar = (int)(AudioManager.Instance.progressTime * 1000 / GameManager.Instance.editorSheet.BarPerMilliSec);
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



    IEnumerator IEMove()
    {
        while (true)
        {
            objects.transform.position += Vector3.down * Time.deltaTime * speed;
            yield return null;
        }
    }

    public void Play(UIObject uiObject)
    {
        Play();
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
            float barPerTime = GameManager.Instance.editorSheet.BarPerSec;
            float pos = time / barPerTime * 16;

            objects.transform.position = new Vector3(0f, -pos + offsetPosition, 0f);

        }
    }

    public void SelectShortNote()
    {
        if (EditorController.Instance.isLongNoteActive)
            EditorController.Instance.isLongNoteActive = false;

        InitNoteCursor();
        EditorController.Instance.isShortNoteActive = !EditorController.Instance.isShortNoteActive;
    }

    public void SelectLongNote()
    {
        if (EditorController.Instance.isShortNoteActive)
            EditorController.Instance.isShortNoteActive = false;

        InitNoteCursor();
        EditorController.Instance.isLongNoteActive = !EditorController.Instance.isLongNoteActive;
    }

    public void InitNoteCursor()
    {
        EditorController.Instance.cursorObj.transform.position = new Vector3(-12, 0, 0);
    }

    public void SheetSave()
    {
        FindObjectOfType<SheetStorage>().SaveSheet();
    }
    public void SheetUpload()
    {
        FindObjectOfType<SheetStorage>().Upload();
    }
    public void SheetDownload()
    {
        FindObjectOfType<SheetStorage>().Download();
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
