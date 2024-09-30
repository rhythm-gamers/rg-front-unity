using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class InputManager : MonoBehaviour
{
    static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject[] keyEffects = new GameObject[6];
    private float[] startTimes = new float[6];
    private RaycastHit2D?[] startNote = new RaycastHit2D?[6];

    public PlayerInput playerInput;

    private Coroutine intervalCoroutine;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Disable()
    {
        playerInput.enabled = false;
    }

    public void SwitchActionMap(string canvasName)
    {
        if (!playerInput.enabled)
            playerInput.enabled = true;

        playerInput.SwitchCurrentActionMap(canvasName);
    }

    private void RunAtIntervalsPressed(Action method)
    {
        if (intervalCoroutine == null)
        {
            intervalCoroutine = StartCoroutine(RunFunctionAtIntervals(method));
        }
    }
    private void RunAtIntervalsReleased()
    {
        if (intervalCoroutine != null)
        {
            StopCoroutine(intervalCoroutine);
            intervalCoroutine = null;
        }
    }
    private IEnumerator RunFunctionAtIntervals(Action method)
    {
        while (true)
        {
            method();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    private void JudgeNote(InputAction.CallbackContext context, int noteIdx)
    {
        if (context.started)
        {
            StartCoroutine(Judgement.Instance.JudgeNote(noteIdx));
            keyEffects[noteIdx].SetActive(true);
        }
        else if (context.canceled)
        {
            StartCoroutine(Judgement.Instance.CheckLongNote(noteIdx));
            keyEffects[noteIdx].SetActive(false);
        }
    }

    private void MakeNote(InputAction.CallbackContext context, int noteIdx)
    {
#if !UNITY_WEBGL
        if (context.started)
        {
            startNote[noteIdx] = EditorController.Instance.FindStartNoteByPress();
            startTimes[noteIdx] = Time.time;
            keyEffects[noteIdx].SetActive(true);
        }
        else if (context.canceled)
        {
            float duration = Time.time - startTimes[noteIdx];
            if (duration < 0.2f)
                EditorController.Instance.MakeShortNoteByPress(startNote[noteIdx], noteIdx);
            else
                EditorController.Instance.MakeLongNoteByPress(startNote[noteIdx], noteIdx);

            keyEffects[noteIdx].SetActive(false);
        }
#endif
    }

    public void OnNoteLine0(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.sheet.keyNum >= 4)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                JudgeNote(context, 0);
            else
                MakeNote(context, 0);
    }
    public void OnNoteLine1(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.sheet.keyNum >= 4)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                JudgeNote(context, 1);
            else
                MakeNote(context, 1);
    }
    public void OnNoteLine2(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.sheet.keyNum >= 4)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                JudgeNote(context, 2);
            else
                MakeNote(context, 2);
    }
    public void OnNoteLine3(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.sheet.keyNum >= 4)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                JudgeNote(context, 3);
            else
                MakeNote(context, 3);
    }
    public void OnNoteLine4(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.sheet.keyNum >= 5)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                JudgeNote(context, 4);
            else
                MakeNote(context, 4);
    }
    public void OnNoteLine5(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.sheet.keyNum >= 6)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                JudgeNote(context, 5);
            else
                MakeNote(context, 5);
    }

    public void OnSpeedDown(InputAction.CallbackContext context)
    {
        if (context.started)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                RunAtIntervalsPressed(Sync.Instance.SpeedDown);
            else return;
        else if (context.canceled)
            RunAtIntervalsReleased();
    }
    public void OnSpeedUp(InputAction.CallbackContext context)
    {
        if (context.started)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                RunAtIntervalsPressed(Sync.Instance.SpeedUp);
            else return;
        else if (context.canceled)
            RunAtIntervalsReleased();
    }
    public void OnJudgeUp(InputAction.CallbackContext context)
    {
        if (context.started)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                RunAtIntervalsPressed(Sync.Instance.JudgeOffsetUp);
            else return;
        else if (context.canceled)
            RunAtIntervalsReleased();
    }
    public void OnJudgeDown(InputAction.CallbackContext context)
    {
        if (context.started)
            if (GameManager.Instance.state == GameManager.GameState.Game)
                RunAtIntervalsPressed(Sync.Instance.JudgeOffsetDown);
            else return;
        else if (context.canceled)
            RunAtIntervalsReleased();
    }

    public void OnEnter(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Game)
            {
                if (GameManager.Instance.isPlayable)
                    GameManager.Instance.Play();
            }
#if !UNITY_WEBGL
            else if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlayable)
                    GameManager.Instance.Edit();
            }
#endif
        }
    }

    public void OnExit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Game)
            {
                if (GameManager.Instance.isPlaying)
                    GameManager.Instance.Pause();
            }
#if !UNITY_WEBGL
            else if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlaying)
                    EditorController.Instance.CheckIsChangedSheet();
            }
#endif
        }
    }

    public void OnMouseBtn(InputAction.CallbackContext context)
    {
#if !UNITY_WEBGL
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
#endif
    }

    public void OnMouseWheel(InputAction.CallbackContext context)
    {
#if !UNITY_WEBGL
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlaying)
                    EditorController.Instance.Scroll(context.ReadValue<float>());
            }
        }
#endif
    }

    public void OnSpace(InputAction.CallbackContext context)
    {
#if !UNITY_WEBGL
        if (context.started)
        {
            if (GameManager.Instance.state == GameManager.GameState.Edit)
                EditorController.Instance.Space();
        }
#endif
    }

    public void OnCtrl(InputAction.CallbackContext context)
    {
#if !UNITY_WEBGL
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
#endif
    }

    public void OnGridOffsetUp(InputAction.CallbackContext context)
    {
#if !UNITY_WEBGL
        if (context.started)
        {
            if (GameManager.Instance.isPlaying)
                GridGenerator.Instance.GridOffsetUp();
        }
#endif
    }
    public void OnGridOffsetDown(InputAction.CallbackContext context)
    {
#if !UNITY_WEBGL
        if (context.started)
        {
            if (GameManager.Instance.isPlaying)
                GridGenerator.Instance.GridOffsetDown();
        }
#endif
    }

    // 게임 바로 끝내기
    public void OnStopGame()
    {
#if !UNITY_WEBGL
        AudioManager.Instance.Stop();
#endif
    }
}
