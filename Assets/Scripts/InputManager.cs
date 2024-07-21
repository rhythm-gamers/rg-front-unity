using UnityEngine;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;

public class InputManager : MonoBehaviour
{
    public GameObject[] keyEffects = new GameObject[4];
    Judgement judgement = null;

    public Vector2 mousePos;

    [DllImport("__Internal")]
    private static extern void SetSpeed(string speed);

    void Start()
    {
        foreach (var effect in keyEffects)
        {
            effect.gameObject.SetActive(false);
        }
        judgement = FindObjectOfType<Judgement>();
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.Edit)
        {
            if (GameManager.Instance.isPlaying)
            {
                mousePos = Mouse.current.position.ReadValue();
            }
        }
    }

    public void OnNoteLine0(InputAction.CallbackContext context)
    {

        if (!GameManager.Instance.isPlayable)
        {
            if (context.started)
            {
                StartCoroutine(judgement.JudgeNote(0));
                keyEffects[0].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(judgement.CheckLongNote(0));
                keyEffects[0].SetActive(false);
            }
        }
    }
    public void OnNoteLine1(InputAction.CallbackContext context)
    {

        if (!GameManager.Instance.isPlayable)
        {
            if (context.started)
            {
                StartCoroutine(judgement.JudgeNote(1));
                keyEffects[1].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(judgement.CheckLongNote(1));
                keyEffects[1].SetActive(false);
            }
        }
    }
    public void OnNoteLine2(InputAction.CallbackContext context)
    {

        if (!GameManager.Instance.isPlayable)
        {
            if (context.started)
            {
                StartCoroutine(judgement.JudgeNote(2));
                keyEffects[2].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(judgement.CheckLongNote(2));
                keyEffects[2].SetActive(false);
            }
        }
    }
    public void OnNoteLine3(InputAction.CallbackContext context)
    {

        if (!GameManager.Instance.isPlayable)
        {
            if (context.started)
            {
                StartCoroutine(judgement.JudgeNote(3));
                keyEffects[3].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(judgement.CheckLongNote(3));
                keyEffects[3].SetActive(false);
            }
        }
    }
    public void OnSpeedDown(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameManager.Instance.Speed -= 0.1f;
            NoteGenerator.Instance.Interpolate();

            string speedToString = GameManager.Instance.Speed.ToString("0.0");
            UIText inGameSpeedUI = UIController.Instance.FindUI("UI_G_Speed").uiObject as UIText;
            UIText outGameSpeedUI = UIController.Instance.FindUI("UI_D_Speed").uiObject as UIText;
            inGameSpeedUI.SetText("Speed " + speedToString);
            outGameSpeedUI.SetText("Speed " + speedToString);
#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
        }
    }
    public void OnSpeedUp(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameManager.Instance.Speed += 0.1f;
            NoteGenerator.Instance.Interpolate();

            string speedToString = GameManager.Instance.Speed.ToString("0.0");
            UIText inGameSpeedUI = UIController.Instance.FindUI("UI_G_Speed").uiObject as UIText;
            UIText outGameSpeedUI = UIController.Instance.FindUI("UI_D_Speed").uiObject as UIText;
            inGameSpeedUI.SetText("Speed " + speedToString);
            outGameSpeedUI.SetText("Speed " + speedToString);
#if UNITY_WEBGL && !UNITY_EDITOR
            SetSpeed(speedToString);
#endif
        }
    }
    public void OnJudgeUp(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.isPlaying)
                Sync.Instance.Up();
        }
    }
    public void OnJudgeDown(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.isPlaying)
                Sync.Instance.Down();
        }
    }

    public void OnEnter(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Game)
            {
                if (GameManager.Instance.isPlayable)
                {
                    GameManager.Instance.Play();
                }
                else if (GameManager.Instance.isPaused)
                {
                    PauseNavigator.Instance.ActivateButton();
                }
            }
            else if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlayable)
                    GameManager.Instance.Edit();
            }
        }
    }
    public void OnExit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Game)
            {
                if (GameManager.Instance.isPlaying)
                {
                    GameManager.Instance.Pause();
                }
                else if (GameManager.Instance.isPaused)
                {
                    GameManager.Instance.UnPause();
                }
            }
            else if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlaying)
                    GameManager.Instance.ExitEditor();
            }
        }
    }

    // 에디터
    public void OnMouseBtn(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlaying)
                {
                    EditorController.Instance.MouseBtn(context.control.name);
                }
            }
        }
    }

    public void OnMouseWheel(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlaying)
                    EditorController.Instance.Scroll(context.ReadValue<float>());
            }
        }
    }

    public void OnSpace(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Edit)
                EditorController.Instance.Space();
        }
    }

    public void OnCtrl(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                EditorController.Instance.isCtrl = true;
                EditorController.Instance.Ctrl();
            }
        }
        else if (context.canceled)
        {
            if (GameManager.Instance.state == GameManager.GameState.Edit)
                EditorController.Instance.isCtrl = false;
        }
    }

    public void OnArrowUp(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.isPaused)
            {
                PauseNavigator.Instance.Navigate(-1);
            }
        }
    }

    public void OnArrowDown(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.isPaused)
            {
                PauseNavigator.Instance.Navigate(1);
            }
        }
    }

    // �׽�Ʈ�� �ڵ�
    public void OnTest(InputAction.CallbackContext context)
    {
        // Audio Time�� ������ �Ű� ���â�� �ٷ� �� �� �ְ� ����
        AudioManager.Instance.audioSource.time = AudioManager.Instance.Length;

        //FindObjectOfType<SheetStorage>().Save();
    }
}
