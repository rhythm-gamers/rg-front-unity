#if !UNITY_WEBGL

using System;
using System.Collections;
using UnityEngine;

public class EditorController : MonoBehaviour
{
    static EditorController instance;
    public static EditorController Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject cursorPrefab;
    public GameObject cursorObj;
    public void SetActiveCursor(bool nextState)
    {
        cursorObj.SetActive(nextState);
    }

    // 4비트, 8비트, 12비트, 16비트, 24비트, 32비트, 48비트, 64비트
    public readonly int[] snap = { 48, 24, 16, 12, 8, 6, 4, 3 };
    int snapIdx = 3; // 16비트 스냅으로 시작
    public int SnapIdx
    {
        get { return snapIdx; }
        set
        {
            snapIdx = Math.Clamp(value, 0, snap.Length - 1);
        }
    }

    public bool isCtrl;
    float scrollValue;
    Coroutine coCtrl;

    Camera cam;
    Vector3 worldPos;

    public Action<int> GridSnapListener;

    GameObject selectedNoteObject;
    Vector3 selectedGridPosition;
    Vector3 lastSelectedGridPosition;
    Transform headTemp;
    int selectedLine = 0;

    int longNoteMakingCount = 0;
    bool isDispose;

    public bool isShortNoteActive;
    public bool isLongNoteActive;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void InitCursor()
    {
        isLongNoteActive = false;
        isShortNoteActive = false;
        SetActiveCursor(false);
    }

    void Start()
    {
        cam = Camera.main;

        cursorObj = Instantiate(cursorPrefab);
        InitCursor();
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.Game) return;
        if (GameManager.Instance.isPlaying == false) return;

        // 그리드에 레이쏴서 위치 알아내야함
        // 현재 스냅에 따라, 스냅될 위치 알아내야함
        //Debug.Log(inputManager.mousePos);
        Vector3 mousePos = EditMouseController.Instance.mousePos;
        Vector3 normalizedMousePos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);

        worldPos = cam.ViewportToWorldPoint(normalizedMousePos);
        worldPos.z = -1.5f;
        //int layerMask = (1 << LayerMask.NameToLayer("Grid")) + (1 << LayerMask.NameToLayer("Note"));

        cursorObj.transform.position = worldPos; // 커서 좌표

        switch (GameManager.Instance.sheet.keyNum)
        {
            case 4:
                Select4KeyLine(worldPos);
                break;
            case 5:
                Select5KeyLine(worldPos);
                break;
            case 6:
                Select6KeyLine(worldPos);
                break;
        }

        Debug.DrawRay(worldPos, cam.transform.forward * 2, Color.red, 0.2f);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, cam.transform.forward, 2f);

        if (hit.transform == null)
        {
            isDispose = false;
            selectedNoteObject = null;
            return;
        }
        else if (hit.transform.CompareTag("Note"))
        {
            // Debug.Log("note");
            isDispose = false;
            selectedNoteObject = hit.transform.gameObject;
        }
        else
        {
            // Debug.Log("grid");
            if (!isShortNoteActive && !isLongNoteActive) return;

            int index = hit.transform.parent.GetComponent<GridObject>().index;
            int beat = int.Parse(hit.transform.name.Split('_')[1]);
            float y = hit.transform.position.y;




            /**
                롱노트 배치 버그 수정 솔루션
                헤드를 찍고 스크롤을 올리거나 내리고
                테일을 찍으면 헤드가 따라올라오거나 내려감..
            */
            if (longNoteMakingCount == 0)
                headTemp = hit.transform; // // head 기억해놨다가 활용

            selectedGridPosition = new Vector3(NoteGenerator.Instance.linePos[selectedLine], y, -1f);
            cursorObj.transform.position = selectedGridPosition;

            selectedNoteObject = null;
            isDispose = true;
        }
    }

    /// <summary>
    /// 스페이스 - 재생/일시정지( Space - Play/Puase )
    /// </summary>
    public void Space()
    {
        Editor.Instance.PlayOrPause();
    }

    /// <summary>
    /// 좌클릭 - 노트 배치 ( Mouse leftBtn - Dispose note )
    /// 우클릭 - 노트 삭제 ( Mouse rightBtn - Cancel note )
    /// </summary>
    /// <param name="btnName"></param>
    public void MouseBtn(string btnName)
    {
        if (btnName == "leftButton")
        {
            if (!isDispose) return;
            if (selectedNoteObject != null)
            {
                Debug.Log("노트가 이미 존재합니다");
                return;
            }

            if (isLongNoteActive)
            {
                if (longNoteMakingCount == 0)
                {
                    lastSelectedGridPosition = selectedGridPosition;

                    NoteGenerator.Instance.DisposeNoteLong(longNoteMakingCount, new Vector3[] { lastSelectedGridPosition, selectedGridPosition });

                    longNoteMakingCount++;
                }
                else if (longNoteMakingCount == 1)
                {
                    Vector3 tailPositon = selectedGridPosition;
                    tailPositon.x = lastSelectedGridPosition.x; // 롱노트는 사선으로 작성될 수 없으므로, 다른 라인(x)에 찍어도 종전과 동일한 위치를 유지
                    lastSelectedGridPosition.y = headTemp.TransformDirection(headTemp.transform.position).y;

                    // tail을 head보다 낮게 배치했을 경우 뒤집어주어야함
                    if (lastSelectedGridPosition.y < tailPositon.y)
                    {
                        NoteGenerator.Instance.DisposeNoteLong(longNoteMakingCount, new Vector3[] { lastSelectedGridPosition, tailPositon });
                    }
                    else
                    {
                        NoteGenerator.Instance.DisposeNoteLong(longNoteMakingCount, new Vector3[] { tailPositon, lastSelectedGridPosition });
                    }

                    longNoteMakingCount = 0;
                }
            }
            else if (isShortNoteActive)
            {
                NoteGenerator.Instance.DisposeNoteShort(selectedGridPosition);
            }
        }
        else if (btnName == "rightButton")
        {
            if (selectedNoteObject != null)
            {
                NoteShort isNoteShortExist = selectedNoteObject.GetComponent<NoteShort>();
                if (!isNoteShortExist)
                {
                    // long은 부모 찾아서 비활성화
                    selectedNoteObject.transform.parent.gameObject.SetActive(false);
                }
                else if (isNoteShortExist)
                {
                    selectedNoteObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 마우스휠 - 음악 및 그리드 위치 이동 ( Mouse wheel - Move music and grids pos )
    /// </summary>
    /// <param name="value"></param>
    public void Scroll(float value)
    {
        scrollValue = value;

        // 스크롤 시 해당 스냅만큼 이동 (컨트롤키가 입력되지않았을때만)
        if (!isCtrl)
        {
            float snapDeltaTime = (float)(GameManager.Instance.sheet.BeatPerSec * snap[SnapIdx]);

            if (scrollValue > 0)
            {
                if (AudioManager.Instance.Length - AudioManager.Instance.progressTime <= 0.1f) return;

                StartCoroutine(AudioManager.Instance.MovePosition(snapDeltaTime));
            }
            else if (scrollValue < 0)
            {
                if (AudioManager.Instance.progressTime == 0f) return;

                StartCoroutine(AudioManager.Instance.MovePosition(-snapDeltaTime));
                //Debug.Log(-GameManager.Instance.sheets[GameManager.Instance.title].BeatPerSec * 0.001f * snap);
            }
        }
    }

    /// <summary>
    /// 컨트롤 + 마우스휠 - 그리드 스냅 변경 ( Ctrl + Mouse wheel - Change snap of grids )
    /// </summary>
    public void Ctrl()
    {
        if (coCtrl != null)
        {
            StopCoroutine(coCtrl);
        }
        coCtrl = StartCoroutine(IEWaitMouseWheel());
    }

    IEnumerator IEWaitMouseWheel()
    {
        while (isCtrl)
        {
            if (scrollValue > 0)
            {
                // 스냅업
                SnapIdx += 1;
                GridSnapListener.Invoke(snap[SnapIdx]);
            }
            else if (scrollValue < 0)
            {
                // 스냅다운
                SnapIdx -= 1;
                GridSnapListener.Invoke(snap[SnapIdx]);
            }
            scrollValue = 0;

            yield return null;
        }
    }

    public void CheckIsChangedSheet()
    {
#if !UNITY_WEBGL
        bool isChangeSheet = SheetStorage.Instance.CompareEditedSheet();
        if (isChangeSheet)
        {
            if (AudioManager.Instance.IsPlaying())
                Editor.Instance.PlayOrPause();

            SetActiveCursor(false);

            PopupController.Instance.InitByScene("Editor",
                () => ExitEditor(),
                () => ResumeEditor());
            PopupController.Instance.SetActiveCanvas(true);
        }
        else
        {
            ExitEditor();
        }
#endif
    }

    private void Select4KeyLine(Vector3 worldPos)
    {
        if (worldPos.x < -1f && worldPos.x > -2f)
        {
            //Debug.Log($"0번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 0;
        }
        else if (worldPos.x < 0f && worldPos.x > -1f)
        {
            //Debug.Log($"1번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 1;
        }
        else if (worldPos.x < 1f && worldPos.x > 0f)
        {
            //Debug.Log($"2번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 2;
        }
        else if (worldPos.x < 2f && worldPos.x > 1f)
        {
            //Debug.Log($"3번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 3;
        }
    }
    private void Select5KeyLine(Vector3 worldPos)
    {
        if (worldPos.x < -1.5f && worldPos.x > -2.5f)
        {
            //Debug.Log($"0번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 0;
        }
        else if (worldPos.x < -0.5f && worldPos.x > -1.5f)
        {
            //Debug.Log($"1번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 1;
        }
        else if (worldPos.x < 0.5f && worldPos.x > -0.5f)
        {
            //Debug.Log($"2번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 2;
        }
        else if (worldPos.x < 1.5f && worldPos.x > 0.5f)
        {
            //Debug.Log($"3번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 3;
        }
        else if (worldPos.x < 2.5f && worldPos.x > 1.5f)
        {
            //Debug.Log($"4번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 4;
        }
    }
    private void Select6KeyLine(Vector3 worldPos)
    {
        if (worldPos.x < -2f && worldPos.x > -3f)
        {
            //Debug.Log($"0번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 0;
        }
        else if (worldPos.x < -1f && worldPos.x > -2f)
        {
            //Debug.Log($"1번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 1;
        }
        else if (worldPos.x < 0f && worldPos.x > -1f)
        {
            //Debug.Log($"2번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 2;
        }
        else if (worldPos.x < 1f && worldPos.x > 0f)
        {
            //Debug.Log($"3번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 3;
        }
        else if (worldPos.x < 2f && worldPos.x > 1f)
        {
            //Debug.Log($"4번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 4;
        }
        else if (worldPos.x < 3f && worldPos.x > 2f)
        {
            //Debug.Log($"5번 레인 : {index}번 그리드 : {beat} 비트");
            selectedLine = 5;
        }
    }

    private void ResumeEditor()
    {
#if !UNITY_WEBGL
        PopupController.Instance.SetActiveCanvas(false);

        InitCursor();
#endif
    }
    private void ExitEditor()
    {
#if !UNITY_WEBGL
        PopupController.Instance.SetActiveCanvas(false);

        // Editor 초기화
        Editor.Instance.Stop();

        // Cursor 초기화
        InitCursor();

        // 그리드 UI 끄기
        GridGenerator.Instance.InActivate();

        // 노트 Gen 끄기
        NoteGenerator.Instance.StopGen();

        AudioManager.Instance.progressTime = 0f;

        GameManager.Instance.Description();
#endif
    }

    //private void OnGUI()
    //{
    //    GUIStyle style = new GUIStyle();
    //    style.fontSize = 36;
    //    GUI.Label(new Rect(100, 100, 100, 100), "Mouse Pos : " + inputManager.mousePos.ToString(), style);
    //    GUI.Label(new Rect(100, 200, 100, 100), "ScreenToWorld : " + worldPos.ToString(), style);
    //    GUI.Label(new Rect(100, 300, 100, 100), "CurrentBar : " + Editor.Instance.currentBar.ToString(), style);
    //    GUI.Label(new Rect(100, 400, 100, 100), "Snap : " + Editor.Instance.Snap.ToString(), style);
    //}
}

#endif