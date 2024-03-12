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

        speed = 16 / GameManager.Instance.sheets[GameManager.Instance.title].BarPerSec;
        offsetPosition = speed * GameManager.Instance.sheets[GameManager.Instance.title].offset * 0.001f;
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
            AudioManager.Instance.Play();
            musicController.SetText("||");
            coMove = StartCoroutine(IEMove());
        }
    }

    public void Stop()
    {
        if (coMove != null)
            StopCoroutine(coMove);

        objects.transform.position = new Vector3(0f, offsetPosition, 0f);
        AudioManager.Instance.progressTime = 0f;
        AudioManager.Instance.Stop();
        musicController.SetText(">");
    }

    public void CaculateCurrnetBar()
    {
        currentBar = (int)(AudioManager.Instance.progressTime * 1000 / GameManager.Instance.sheets[GameManager.Instance.title].BarPerMilliSec);
    }

    IEnumerator IEBarTimer()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            CaculateCurrnetBar();
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

    public void Progress()
    {
        if (slider != null)
        {
            float time = AudioManager.Instance.Length * slider.slider.value;
            AudioManager.Instance.progressTime = time;

            // ���� Ÿ�ӿ� ���缭 ������Ʈ�� �̵�
            // �Ѹ��� 16�� �̵�
            // time / �Ѹ��� �ð�

            CaculateCurrnetBar();

            // �� �׸���(�� ����)�� ���ӿ�����Ʈ y��ǥ�� ���̴� 16
            // ���� ������ġ * 16 = ����s
            float barPerTime = GameManager.Instance.sheets[GameManager.Instance.title].BarPerSec;
            float pos = time / barPerTime * 16;

            objects.transform.position = new Vector3(0f, -pos + offsetPosition, 0f);

        }
    }

    public void SelectShortNote()
    {
        if (EditorController.Instance.isLongNoteActive)
        {
            EditorController.Instance.isLongNoteActive = false;
        }
        EditorController.Instance.isShortNoteActive = !EditorController.Instance.isShortNoteActive;
    }

    public void SelectLongNote()
    {
        if (EditorController.Instance.isShortNoteActive)
        {
            EditorController.Instance.isShortNoteActive = false;
        }
        EditorController.Instance.isLongNoteActive = !EditorController.Instance.isLongNoteActive;
    }

    public void FileSave()
    {
        FindObjectOfType<SheetStorage>().Save();
    }
}
