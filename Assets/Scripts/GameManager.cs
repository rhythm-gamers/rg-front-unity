using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public GameObject[] keyEffects = new GameObject[6];

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

    IEnumerator WebGLInitUserSpeed(float userSpeed)
    {
        speed = userSpeed;

        yield return new WaitUntil(() => UIController.Instance.isInit == true);
        UIText inGameSpeedUI = UIController.Instance.FindUI("UI_G_Speed").uiObject as UIText;
        UIText outGameSpeedUI = UIController.Instance.FindUI("UI_D_Speed").uiObject as UIText;
        inGameSpeedUI.SetText(Speed.ToString("0.0"));
        outGameSpeedUI.SetText(Speed.ToString("0.0"));
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
        InputManager.Instance.Disable();
        StartCoroutine(IEDescription());
    }

    public void Play()
    {
        InputManager.Instance.Disable();
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
    // Finish Input Manager Method

    public void ChangeGameMode()
    {
#if !UNITY_WEBGL
        UIButton GameModeBtn = UIController.Instance.GetUI("UI_D_GameMode").uiObject as UIButton;
        UIText SpeedUI = UIController.Instance.GetUI("UI_D_Speed").uiObject as UIText;
        UIText OffsetUI = UIController.Instance.GetUI("UI_D_JudgeOffset").uiObject as UIText;

        if (state == GameState.Game)
        {
            state = GameState.Edit;
            GameModeBtn.SetText("Edit Mode");
            SpeedUI.transform.parent.gameObject.SetActive(false);
            OffsetUI.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            state = GameState.Game;
            GameModeBtn.SetText("Game Mode");
            SpeedUI.transform.parent.gameObject.SetActive(true);
            OffsetUI.transform.parent.gameObject.SetActive(true);
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
        canvases[(int)Canvas.SelectSheet].SetActive(false);

        Score.Instance.Init();

        // 채보 로드
        SheetLoader.Instance.Init();

        yield return new WaitUntil(() => SheetLoader.Instance.isLoadFinish == true);

        // BGA 설정
        canvases[(int)Canvas.GameBGA].GetComponentInChildren<BGA>().Init();

        // 선택화면 채보 정보 초기화
        ItemController.Instance.Init();

        // 바뀐 keyNum에 대해 오브젝트들 초기화
        KeyNumChangeController.Instance.Init();

        // 판정선 오프셋 오브젝트들 초기화
        Sync.Instance.Init();

#if !UNITY_WEBGL || UNITY_EDITOR
        FindObjectOfType<RebindController>().Init(); // 4, 5, 6키 각각 기본 키세팅으로 초기화
#endif

        canvases[(int)Canvas.Title].SetActive(true);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

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

        // UI들 끄기
        canvases[(int)Canvas.Editor].SetActive(false);
        canvases[(int)Canvas.Title].SetActive(false);
        canvases[(int)Canvas.Result].SetActive(false);
        canvases[(int)Canvas.SelectSheet].SetActive(false);
        canvases[(int)Canvas.WarningPopup].SetActive(false);

        // BGA 켜기
        canvases[(int)Canvas.GameBGA].SetActive(true);

        // Description UI 켜기
        canvases[(int)Canvas.Description].SetActive(true);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // 새 게임을 시작할 수 있게 해줌
        isPlayable = true;

        InputManager.Instance.SwitchActionMap("Description");
    }

    IEnumerator IEInitPlay()
    {
        // 새 게임을 시작할 수 없게 해줌
        isPlayable = false;

        // 게임 재시작 시 초기화 옵션
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        isPlaying = false;
        AudioManager.Instance.Stop();
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
        InputManager.Instance.SwitchActionMap("Game");

        // 판정 초기화
        Judgement.Instance.Init();

        // 점수 초기화
        Score.Instance.Clear();

        // 판정 이펙트 초기화
        JudgeEffect.Instance.Init();

        // 노트 생성 중지
        NoteGenerator.Instance.StopGen();

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 2f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // Late Miss 판정 체크
        Judgement.Instance.StartMissCheck();

        // Note 생성
        NoteGenerator.Instance.StartGen();

        // 2초 대기
        yield return new WaitForSeconds(2f);

        // 게임 시작
        isPlaying = true;

        // Audio 재생
        AudioManager.Instance.Play();
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

        // 게임 결과창 초기화
        GameResultController.Instance.Init();

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

        InputManager.Instance.SwitchActionMap("Result");
    }


    // 에디터 전용 메서드
#if !UNITY_WEBGL
    public void EditorMenu()
    {
        InputManager.Instance.Disable();
        StartCoroutine(IEEditorMenu());
    }

    public void WriteSheet()
    {
        InputManager.Instance.Disable();
        StartCoroutine(IEWriteSheet());
    }

    public void SelectSheet()
    {
        InputManager.Instance.Disable();
        StartCoroutine(IESelectSheet());
    }

    public void SelectSheet(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InputManager.Instance.Disable();
            StartCoroutine(IESelectSheet());
        }
    }

    public void Edit()
    {
        InputManager.Instance.Disable();
        StartCoroutine(IEEdit());
    }


    void ActiveMasterUI()
    {
        UIButton GameModeBtn = UIController.Instance.GetUI("UI_D_GameMode").uiObject as UIButton;
        UIText EditorHotkeyUI = UIController.Instance.GetUI("UI_G_EditorHotkey").uiObject as UIText;
        UIText SpeedUI = UIController.Instance.GetUI("UI_D_Speed").uiObject as UIText;
        UIText OffsetUI = UIController.Instance.GetUI("UI_D_JudgeOffset").uiObject as UIText;

        GameModeBtn.SetText("Edit Mode");

        GameModeBtn.gameObject.SetActive(true);
        EditorHotkeyUI.gameObject.SetActive(true);
        SpeedUI.transform.parent.gameObject.SetActive(false);
        OffsetUI.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator IEEditorMenu()
    {
        // Title UI, WriteSheet UI, SelectSheet UI 끄기
        canvases[(int)Canvas.Title].SetActive(false);
        canvases[(int)Canvas.WriteSheet].SetActive(false);
        canvases[(int)Canvas.SelectSheet].SetActive(false);

        // Editor Menu UI 켜기
        canvases[(int)Canvas.EditorMenu].SetActive(true);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

        InputManager.Instance.SwitchActionMap("EditorMenu");
    }

    IEnumerator IEWriteSheet()
    {
        // EditorMenu UI 끄기
        canvases[(int)Canvas.EditorMenu].SetActive(false);

        // WriteSheet UI 켜기
        canvases[(int)Canvas.WriteSheet].SetActive(true);

        InputManager.Instance.SwitchActionMap("WriteSheet");
        yield return null;
    }

    IEnumerator IESelectSheet()
    {
        // 화면 페이드 아웃
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 3f));

        yield return new WaitUntil(() => SheetStorage.Instance.isNewSheetAdded);

        // EditorMenu UI 끄기
        canvases[(int)Canvas.EditorMenu].SetActive(false);
        canvases[(int)Canvas.Description].SetActive(false);
        canvases[(int)Canvas.WriteSheet].SetActive(false);

        // SelectSheet UI 켜기
        canvases[(int)Canvas.SelectSheet].SetActive(true);

        // 저장된 파일 Preload
        SavedFilesReader.Instance.PreloadSavedFilesAsync();

        yield return new WaitUntil(() => SavedFilesReader.Instance.isSavedFilesLoaded);

        SavedFilesReader.Instance.ReadFiles();

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f, SavedFilesReader.Instance.isSavedFilesLoaded));
        canvases[(int)Canvas.SFX].SetActive(false);

        InputManager.Instance.SwitchActionMap("SelectSheet");
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

        // Editor UI 켜기
        canvases[(int)Canvas.Editor].SetActive(true);

        // 화면 페이드 인
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 3f));
        canvases[(int)Canvas.SFX].SetActive(false);

        InputManager.Instance.SwitchActionMap("Editor");
    }
#endif
}
