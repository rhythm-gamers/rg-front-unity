using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    public enum GameState
    {
        Game,
        Edit,
    }
    public GameState state;

    /// <summary>
    /// 게임 진행 상태. InputManager.OnEnter() 참고
    /// </summary>
    public bool isPlaying = false;
    public bool isPlayable = false;
    public bool isPaused = false;
    public string title;
    Coroutine coPlaying;

    public GameObject[] keyEffects = new GameObject[4];

    public Sheet sheet = new();

    float speed = 2.5f;
    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            speed = Mathf.Clamp(value, 1.0f, 5.0f);
        }
    }

    private InputActions inputActions;

    IEnumerator WebGLInitUserSpeed(float userSpeed)
    {
        speed = userSpeed;

        yield return new WaitUntil(() => UIController.Instance.isInit == true);
        UIText inGameSpeedUI = UIController.Instance.FindUI("UI_G_Speed").uiObject as UIText;
        UIText outGameSpeedUI = UIController.Instance.FindUI("UI_D_Speed").uiObject as UIText;
        inGameSpeedUI.SetText("Speed " + Speed.ToString("0.0"));
        outGameSpeedUI.SetText("Speed " + Speed.ToString("0.0"));
    }

    public List<GameObject> canvases = new();

    enum Canvas
    {
        Title,
        Description,
        SFX,
        GameBGA,
        Game,
        Pause,
        Result,
        Editor,
        WarningPopup,
        EditorMenu,
        WriteSheet,
        SelectSheet
    }
    CanvasGroup sfxFade;

    void Awake()
    {
        if (instance == null)
            instance = this;

        inputActions = new InputActions();
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        StartCoroutine(IEInit());
    }

    public void Title()
    {
        StartCoroutine(IETitle());
    }

    public void Description()
    {
        StartCoroutine(IEDescription());
    }

    public void Play()
    {
        StartCoroutine(IEInitPlay());
    }


    // Button Navigator OnClick
    public void UnPause()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        // Pause UI 끄기
        canvases[(int)Canvas.Pause].SetActive(false);

        // Audio 다시 재싱
        AudioManager.Instance.UnPause();

        Judgement.Instance.StartMissCheck();

        // End 알리미
        coPlaying = StartCoroutine(IEEndPlay());
    }

    public void Retry()
    {
        // Game UI 끄기
        canvases[(int)Canvas.Game].SetActive(false);

        // Pause UI 끄기
        canvases[(int)Canvas.Pause].SetActive(false);

        StartCoroutine(IEInitPlay());
    }

    public void ResumeEditor()
    {
#if !UNITY_WEBGL
        canvases[(int)Canvas.WarningPopup].SetActive(false);

        EditorController.Instance.InitCursor();
#endif
    }
    public void ExitEditor()
    {
#if !UNITY_WEBGL
        canvases[(int)Canvas.WarningPopup].SetActive(false);

        // Editor 초기화
        Editor.Instance.Stop();

        // Cursor 초기화
        EditorController.Instance.InitCursor();

        // 그리드 UI 끄기
        GridGenerator.Instance.InActivate();

        // 노트 Gen 끄기
        NoteGenerator.Instance.StopGen();

        AudioManager.Instance.progressTime = 0f;

        Description();
#endif
    }
    // Finish Button Navigator OnClick

    // Input Manager Method
    public void Pause()
    {
        Time.timeScale = 0f;

        // Pause UI 켜기
        canvases[(int)Canvas.Pause].SetActive(true);

        Judgement.Instance.StopMissCheck();

        // playing timer 끄기
        if (coPlaying != null)
        {
            StopCoroutine(coPlaying);
            coPlaying = null;
        }

        // 음악 멈추기
        AudioManager.Instance.Pause();
    }

    public void CheckIsChangedSheet()
    {
#if !UNITY_WEBGL
        bool isChangeSheet = SheetStorage.Instance.CompareEditedSheet();
        if (isChangeSheet)
        {
            if (AudioManager.Instance.IsPlaying())
            {
                Editor.Instance.PlayOrPause();
            }

            EditorController.Instance.SetActiveCursor(false);
            canvases[(int)Canvas.WarningPopup].SetActive(true);
        }
        else
        {
            ExitEditor();
        }
#endif
    }
    // Finish Input Manager Method

    public void ChangeGameMode()
    {
#if !UNITY_WEBGL
        UIButton GameModeBtn = UIController.Instance.GetUI("UI_D_GameMode").uiObject as UIButton;
        UIText SpeedUI = UIController.Instance.GetUI("UI_D_Speed").uiObject as UIText;
        UIText OffsetUI = UIController.Instance.GetUI("UI_D_Offset").uiObject as UIText;

        if (state == GameState.Game)
        {
            state = GameState.Edit;
            Sync.Instance.ResetJudgeOffset();

            GameModeBtn.SetText("Edit Mode");
            SpeedUI.gameObject.SetActive(false);
            OffsetUI.gameObject.SetActive(false);
        }
        else
        {
            state = GameState.Game;
            GameModeBtn.SetText("Game Mode");
            SpeedUI.gameObject.SetActive(true);
            OffsetUI.gameObject.SetActive(true);
        }
#endif
    }

    IEnumerator IEInit()
    {
#if UNITY_WEBGL
        state = GameState.Game;
#else
        state = GameState.Edit;
#endif

        foreach (GameObject go in canvases)
        {
            go.SetActive(true);
        }
        sfxFade = canvases[(int)Canvas.SFX].GetComponent<CanvasGroup>();
        sfxFade.alpha = 1f;

        UIController.Instance.Init();

        // UIObject들이 자기자신을 캐싱할때까지 여유를 주고 비활성화(임시코드)
        yield return new WaitForSecondsRealtime(2f);

        // 에디터 전용 UI 끄기
        UIButton GameModeUI = UIController.Instance.GetUI("UI_D_GameMode").uiObject as UIButton;
        UIText EditorHotkeyUI = UIController.Instance.GetUI("UI_G_EditorHotkey").uiObject as UIText;
        GameModeUI.gameObject.SetActive(false);
        EditorHotkeyUI.gameObject.SetActive(false);

        foreach (var effect in keyEffects)
        {
            effect.SetActive(false);
        }

        canvases[(int)Canvas.Game].SetActive(false);
        canvases[(int)Canvas.Pause].SetActive(false);
        canvases[(int)Canvas.GameBGA].SetActive(false);
        canvases[(int)Canvas.Result].SetActive(false);
        canvases[(int)Canvas.Description].SetActive(false);
        canvases[(int)Canvas.Editor].SetActive(false);
        canvases[(int)Canvas.WarningPopup].SetActive(false);
        canvases[(int)Canvas.EditorMenu].SetActive(false);
        canvases[(int)Canvas.WriteSheet].SetActive(false);
        canvases[(int)Canvas.SelectSheet].SetActive(false);

#if UNITY_WEBGL
        // 타이틀 화면 시작
        Title();
#else
        // 에디터 메뉴 화면 시작
        ActiveMasterUI();
        EditorMenu();
#endif
    }

    IEnumerator IETitle()
    {
        canvases[(int)Canvas.SFX].SetActive(false);
        canvases[(int)Canvas.SelectSheet].SetActive(false);

        canvases[(int)Canvas.Title].SetActive(true);

        Score.Instance.Init();

        // 채보 로드
        SheetLoader.Instance.Init();

        yield return new WaitUntil(() => SheetLoader.Instance.isLoadFinish == true);

        // BGA 설정
        canvases[(int)Canvas.GameBGA].GetComponentInChildren<BGA>().Init();

        // 선택화면 채보 정보 초기화
        ItemController.Instance.Init();

        // 판정선 오프셋 오브젝트들 초기화
        Sync.Instance.Init();

        // 화면 페이드 인
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));

        // 타이틀 인트로 재생
        canvases[(int)Canvas.Title].GetComponent<Animation>().Play();
        yield return new WaitForSeconds(4f);

        // 선택화면 시작
        Description();
    }

    IEnumerator IEDescription()
    {
        // 값 초기화
        isPlaying = false;

        // 화면 페이드 아웃
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 3f));

        // 노트 생성 중지
        NoteGenerator.Instance.StopGen();

        // Editor UI 끄기
        canvases[(int)Canvas.Editor].SetActive(false);

        // Title UI 끄기
        canvases[(int)Canvas.Title].SetActive(false);

        // Result UI 끄기
        canvases[(int)Canvas.Result].SetActive(false);

        // SelectSheet UI 끄기
        canvases[(int)Canvas.SelectSheet].SetActive(false);

        // BGA 켜기
        canvases[(int)Canvas.GameBGA].SetActive(true);

        // Description UI 켜기
        canvases[(int)Canvas.Description].SetActive(true);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // 새 게임을 시작할 수 있게 해줌
        isPlayable = true;
    }

    IEnumerator IEInitPlay()
    {
        // 새 게임을 시작할 수 없게 해줌
        isPlayable = false;

        // 게임 재시작 시 초기화 옵션
        if (Time.timeScale == 0) Time.timeScale = 1;
        isPlaying = false;
        AudioManager.Instance.Pause();
        AudioManager.Instance.progressTime = 0f;

        // 화면 페이드 아웃
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 2f));

        //  Description UI 끄기
        canvases[(int)Canvas.Description].SetActive(false);

#if !UNITY_WEBGL
        SheetStorage.Instance.Init(); // 에디팅 후 테스트 시, 임시 저장된 채보를 사용
#endif

        // Sheet 초기화
        sheet.Init();

        // Audio 삽입
        AudioManager.Instance.Insert(sheet.clip);

        // Game UI 켜기
        canvases[(int)Canvas.Game].SetActive(true);

        // 판정 초기화
        Judgement.Instance.Init();

        // 점수 초기화
        Score.Instance.Clear();

        // 판정 이펙트 초기화
        JudgeEffect.Instance.Init();

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 2f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // Note 생성
        NoteGenerator.Instance.StartGen();

        // Late Miss 판정 체크
        Judgement.Instance.StartMissCheck();

        // 2초 대기
        yield return new WaitForSeconds(2f);

        // 게임 시작
        isPlaying = true;

        // Audio 재생
        AudioManager.Instance.Play();

        // 대기 시간동안 바뀐 배속을 노트에 반영
        NoteGenerator.Instance.Interpolate();

        // End 알리미
        coPlaying = StartCoroutine(IEEndPlay());
    }

    // 게임 끝
    IEnumerator IEEndPlay()
    {
        while (true)
        {
            if (AudioManager.Instance.state == AudioManager.State.Stop)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
        }

        isPlaying = false;

        // 화면 페이드 아웃
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 3f));
        canvases[(int)Canvas.Game].SetActive(false);
        canvases[(int)Canvas.Pause].SetActive(false);
        canvases[(int)Canvas.Result].SetActive(true);

        // 노트 생성 중지
        NoteGenerator.Instance.StopGen();

        UIText rscore = UIController.Instance.FindUI("UI_R_Score").uiObject as UIText;
        UIText rrhythm = UIController.Instance.FindUI("UI_R_Rhythm").uiObject as UIText;
        UIText rgreat = UIController.Instance.FindUI("UI_R_Great").uiObject as UIText;
        UIText rgood = UIController.Instance.FindUI("UI_R_Good").uiObject as UIText;
        UIText rfastmiss = UIController.Instance.FindUI("UI_R_FastMiss").uiObject as UIText;
        UIText rtotalmiss = UIController.Instance.FindUI("UI_R_TotalMiss").uiObject as UIText;
        UIText rslowmiss = UIController.Instance.FindUI("UI_R_SlowMiss").uiObject as UIText;

        rscore.SetText(Score.Instance.data.score.ToString());
        rrhythm.SetText(Score.Instance.data.rhythm.ToString());
        rgreat.SetText(Score.Instance.data.great.ToString());
        rgood.SetText(Score.Instance.data.good.ToString());
        rfastmiss.SetText(Score.Instance.data.fastMiss.ToString());
        rtotalmiss.SetText(Score.Instance.data.miss.ToString());
        rslowmiss.SetText(Score.Instance.data.slowMiss.ToString());

        Debug.Log($"총 인식된 노트수: {Score.Instance.data.rhythm + Score.Instance.data.great + Score.Instance.data.good + Score.Instance.data.miss}");

        UIImage rBG = UIController.Instance.FindUI("UI_R_BG").uiObject as UIImage;
        rBG.SetSprite(sheet.img);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // 사용자 엔터 대기
        yield return new WaitUntil(() => inputActions.Player.Enter.triggered);

        // 선택 화면 불러오기
        Description();
    }


    // 에디터 전용 메서드
#if !UNITY_WEBGL
    public void EditorMenu()
    {
        StartCoroutine(IEEditorMenu());
    }

    public void WriteSheet()
    {
        StartCoroutine(IEWriteSheet());
    }

    public void SelectSheet()
    {
        StartCoroutine(IESelectSheet());
    }

    public void Edit()
    {
        StartCoroutine(IEEdit());
    }


    void ActiveMasterUI()
    {
        UIButton GameModeBtn = UIController.Instance.GetUI("UI_D_GameMode").uiObject as UIButton;
        UIText EditorHotkeyUI = UIController.Instance.GetUI("UI_G_EditorHotkey").uiObject as UIText;
        UIText SpeedUI = UIController.Instance.GetUI("UI_D_Speed").uiObject as UIText;
        UIText OffsetUI = UIController.Instance.GetUI("UI_D_Offset").uiObject as UIText;

        GameModeBtn.SetText("Edit Mode");

        GameModeBtn.gameObject.SetActive(true);
        EditorHotkeyUI.gameObject.SetActive(true);
        SpeedUI.gameObject.SetActive(false);
        OffsetUI.gameObject.SetActive(false);
    }

    IEnumerator IEEditorMenu()
    {
        // Title UI, WriteSheet UI 끄기
        canvases[(int)Canvas.Title].SetActive(false);
        canvases[(int)Canvas.WriteSheet].SetActive(false);

        // Editor Menu UI 켜기
        canvases[(int)Canvas.EditorMenu].SetActive(true);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);
    }

    IEnumerator IEWriteSheet()
    {
        // EditorMenu UI 끄기
        canvases[(int)Canvas.EditorMenu].SetActive(false);

        // WriteSheet UI 켜기
        canvases[(int)Canvas.WriteSheet].SetActive(true);
        yield return null;
    }

    IEnumerator IESelectSheet()
    {
        // 화면 페이드 아웃
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 3f));

        // EditorMenu UI 끄기
        canvases[(int)Canvas.EditorMenu].SetActive(false);
        canvases[(int)Canvas.Description].SetActive(false);

        // 저장된 파일 Read
        SavedFilesReader.Instance.ReadFiles();

        // SelectSheet UI 켜기
        canvases[(int)Canvas.SelectSheet].SetActive(true);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);
    }

    IEnumerator IEEdit()
    {
        yield return new WaitUntil(() => isPlayable);

        // 새 게임을 시작할 수 없게 해줌
        isPlaying = true;
        isPlayable = false;

        // 화면 페이드 아웃
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 3f));

        //  EditorMenu, WriteSheet, Description UI 끄기
        canvases[(int)Canvas.EditorMenu].SetActive(false);
        canvases[(int)Canvas.WriteSheet].SetActive(false);
        canvases[(int)Canvas.Description].SetActive(false);

        // Sheet 및 Sheet Storage 초기화
        SheetStorage.Instance.Init();
        sheet.Init();

        // Audio 삽입 및 초기화
        AudioManager.Instance.Insert(sheet.clip);
        AudioManager.Instance.InitForEdit();

        // Grid 생성
        GridGenerator.Instance.Init();

        // Editor 초기화
        Editor.Instance.Init();

        // Note 생성
        NoteGenerator.Instance.GenAll();

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // Editor UI 켜기
        canvases[(int)Canvas.Editor].SetActive(true);
    }
#endif
}
