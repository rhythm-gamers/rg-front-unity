using UnityEngine;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;
using System.Collections;
using System;

public class InputManager : MonoBehaviour
{
    public GameObject[] keyEffects = new GameObject[4];

    private Coroutine intervalCoroutine;

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



    public void OnNoteLine0(InputAction.CallbackContext context)
    {

        if (!GameManager.Instance.isPlayable)
        {
            if (context.started)
            {
                StartCoroutine(Judgement.Instance.JudgeNote(0));
                keyEffects[0].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(Judgement.Instance.CheckLongNote(0));
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
                StartCoroutine(Judgement.Instance.JudgeNote(1));
                keyEffects[1].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(Judgement.Instance.CheckLongNote(1));
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
                StartCoroutine(Judgement.Instance.JudgeNote(2));
                keyEffects[2].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(Judgement.Instance.CheckLongNote(2));
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
                StartCoroutine(Judgement.Instance.JudgeNote(3));
                keyEffects[3].SetActive(true);
            }
            else if (context.canceled)
            {
                StartCoroutine(Judgement.Instance.CheckLongNote(3));
                keyEffects[3].SetActive(false);
            }
        }
    }
    public void OnSpeedDown(InputAction.CallbackContext context)
    {
        if (context.started)
            RunAtIntervalsPressed(Sync.Instance.SpeedDown);
        else if (context.canceled)
            RunAtIntervalsReleased();
    }
    public void OnSpeedUp(InputAction.CallbackContext context)
    {
        if (context.started)
            RunAtIntervalsPressed(Sync.Instance.SpeedUp);
        else if (context.canceled)
            RunAtIntervalsReleased();
    }
    public void OnJudgeUp(InputAction.CallbackContext context)
    {
        if (context.started)
            RunAtIntervalsPressed(Sync.Instance.JudgeOffsetUp);
        else if (context.canceled)
            RunAtIntervalsReleased();
    }
    public void OnJudgeDown(InputAction.CallbackContext context)
    {
        if (context.started)
            RunAtIntervalsPressed(Sync.Instance.JudgeOffsetDown);
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

            else if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                if (GameManager.Instance.isPlaying)
                    GameManager.Instance.CheckIsChangedSheet();
            }
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
