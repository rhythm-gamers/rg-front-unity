using UnityEngine;

public class KeyNumChangeController : MonoBehaviour
{
    static KeyNumChangeController instance;
    public static KeyNumChangeController Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject lane;
    public GameObject judgeLine;
    public GameObject judgeEffects;
    public GameObject keyEffects;
    public GameObject Bottom;

    // 에디터에서 사용
    public GameObject grids;
    public GameObject noteButtons;
    public GameObject progressBar;
    public GameObject timer;

    private Vector3 noteButtonsLocalPos;
    private Vector3 progressBarLocalPos;
    private Vector3 timerLocalPos;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        noteButtonsLocalPos = noteButtons.transform.localPosition;
        progressBarLocalPos = progressBar.transform.localPosition;
        timerLocalPos = timer.transform.localPosition;
    }

    public void Init()
    {
        switch (GameManager.Instance.sheet.keyNum)
        {
            case 4:
                judgeEffects.transform.localPosition = Vector3.right;
                keyEffects.transform.localPosition = Vector3.right;

                lane.transform.localScale = new Vector3(4, 12, 1);
                judgeLine.transform.localScale = new Vector3(4, 0.05f, 1);
                Bottom.transform.localScale = new Vector3(4, 16, 1);
#if !UNITY_WEBGL
                grids.transform.localScale = Vector3.one;
                noteButtons.transform.localPosition = noteButtonsLocalPos;
                progressBar.transform.localPosition = progressBarLocalPos;
                timer.transform.localPosition = timerLocalPos;
                NoteGenerator.Instance.linePos = new float[] { -1.5f, -0.5f, 0.5f, 1.5f };
#endif
                break;

            case 5:
                judgeEffects.transform.localPosition = Vector3.right * 0.5f;
                keyEffects.transform.localPosition = Vector3.right * 0.5f;

                lane.transform.localScale = new Vector3(5, 12, 1);
                judgeLine.transform.localScale = new Vector3(5, 0.05f, 1);
                Bottom.transform.localScale = new Vector3(5, 16, 1);
#if !UNITY_WEBGL
                grids.transform.localScale = new Vector3(1.25f, 1, 1);
                noteButtons.transform.localPosition = noteButtonsLocalPos + Vector3.left * 50;
                progressBar.transform.localPosition = progressBarLocalPos + Vector3.right * 50;
                timer.transform.localPosition = timerLocalPos + Vector3.right * 50;
                NoteGenerator.Instance.linePos = new float[] { -2f, -1f, 0f, 1f, 2f };
#endif
                break;

            case 6:
                judgeEffects.transform.localPosition = Vector3.zero;
                keyEffects.transform.localPosition = Vector3.zero;

                lane.transform.localScale = new Vector3(6, 12, 1);
                judgeLine.transform.localScale = new Vector3(6, 0.05f, 1);
                Bottom.transform.localScale = new Vector3(6, 16, 1);
#if !UNITY_WEBGL
                grids.transform.localScale = new Vector3(1.5f, 1, 1);
                noteButtons.transform.localPosition = noteButtonsLocalPos + Vector3.left * 100;
                progressBar.transform.localPosition = progressBarLocalPos + Vector3.right * 100;
                timer.transform.localPosition = timerLocalPos + Vector3.right * 100;
                NoteGenerator.Instance.linePos = new float[] { -2.5f, -1.5f, -0.5f, 0.5f, 1.5f, 2.5f };
#endif
                break;
        }
    }
}
