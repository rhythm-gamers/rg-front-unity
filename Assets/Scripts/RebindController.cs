using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RebindController : MonoBehaviour
{
    public InputActionReference[] note0ActionRefs = new InputActionReference[2];
    public InputActionReference[] note1ActionRefs = new InputActionReference[2];
    public InputActionReference[] note2ActionRefs = new InputActionReference[2];
    public InputActionReference[] note3ActionRefs = new InputActionReference[2];
    public InputActionReference[] note4ActionRefs = new InputActionReference[2];
    public InputActionReference[] note5ActionRefs = new InputActionReference[2];

    private Dictionary<string, string> specialKeyMappings = new Dictionary<string, string>
{
    { ";", "semicolon" },
    { "'", "quote" },
    { "[", "leftBracket" },
    { "]", "rightBracket" },
    { ".", "period" },
    { ",", "comma" },
    { "/", "slash" },
    { "-", "minus" },
    { "=", "equals" },
    { "\\", "backslash" },
    { "Alt", "leftAlt" },
    { "ArrowUp", "upArrow" },
    { "ArrowDown", "downArrow" },
    { "ArrowLeft", "leftArrow" },
    { "ArrowRight", "rightArrow" },
    { "Backspace", "backspace" },
    { "CapsLock", "capsLock" },
    { "Ctrl", "leftCtrl" },
    { "Delete", "delete" },
    { "End", "end" },
    { "Enter", "return" },
    { "HangulMode", "imeLangToggle" },
    { "HanjaMode", "imeConvert" },
    { "Home", "home" },
    { "Insert", "insert" },
    { "LeftShift", "leftShift" },
    { "PageDown", "pageDown" },
    { "PageUp", "pageUp" },
    { "RightShift", "rightShift" },
    { "Space", "space" }
};

#if !UNITY_WEBGL || UNITY_EDITOR
    public void Init()
    {
        string[] fourKeys = { "a", "s", ";", "'" };
        string[] fiveKeys = { "a", "s", "d", "l", ";", "'" };
        string[] sixKeys = { "a", "s", "d", "l", ";", "'" };

        switch (GameManager.Instance.sheet.keyNum)
        {
            case 4:
                for (int i = 0; i < fourKeys.Length; i++)
                {
                    StartCoroutine(IERebindNoteAction(i, fourKeys[i]));
                }
                break;
            case 5:
                for (int i = 0; i < fiveKeys.Length; i++)
                {
                    if (i < 3)
                        StartCoroutine(IERebindNoteAction(i, fiveKeys[i]));
                    else if (i == 3)
                        StartCoroutine(IEAddNoteAction(2, fiveKeys[i]));
                    else
                        StartCoroutine(IERebindNoteAction(i - 1, fiveKeys[i]));
                }
                break;
            case 6:
                for (int i = 0; i < sixKeys.Length; i++)
                {
                    StartCoroutine(IERebindNoteAction(i, sixKeys[i]));
                }
                break;
        }
    }
#endif

    public void WebGLRebindNoteKey(string combinedArgs)
    {
        var args = combinedArgs.Split(',');
        int.TryParse(args[0], out int noteLine);
        string newKey = args[1];

        switch (GameManager.Instance.sheet.keyNum)
        {
            case 4:
                StartCoroutine(IERebindNoteAction(noteLine, newKey));
                break;
            case 5:
                if (noteLine < 3)
                    StartCoroutine(IERebindNoteAction(noteLine, newKey));
                else if (noteLine == 3)
                    StartCoroutine(IEAddNoteAction(2, newKey));
                else
                    StartCoroutine(IERebindNoteAction(noteLine - 1, newKey));
                break;
            case 6:
                StartCoroutine(IERebindNoteAction(noteLine, newKey));
                break;
        }
    }

    private IEnumerator IERebindNoteAction(int noteLine, string newKey)
    {
        InputActionReference[] noteActionRefs = FindNoteActionRef(noteLine);
        if (noteActionRefs == null)
        {
            Debug.LogError($"noteActionRef is null for noteLine: {noteLine}");
            yield break;
        }

        foreach (InputActionReference noteActionRef in noteActionRefs)
        {
            InputAction noteAction = noteActionRef.action;
            if (noteAction == null)
            {
                Debug.LogError($"noteAction is null for noteLine: {noteLine}");
                yield break;
            }

            noteAction.Disable();
            noteAction.RemoveAllBindingOverrides();

            // 문자가 특수문자일 경우, 맵핑 사용
            if (specialKeyMappings.ContainsKey(newKey))
            {
                newKey = specialKeyMappings[newKey];
            }

            noteAction.ApplyBindingOverride(0, $"<Keyboard>/{newKey}");
            noteAction.Enable();
        }

        Debug.Log($"Rebound Note{noteLine} to <Keyboard>/{newKey}");
        yield break;
    }

    private IEnumerator IEAddNoteAction(int noteLine, string newKey)
    {
        InputActionReference[] noteActionRefs = FindNoteActionRef(noteLine);
        if (noteActionRefs == null)
        {
            Debug.LogError($"noteActionRef is null for noteLine: {noteLine}");
            yield break;
        }

        foreach (InputActionReference noteActionRef in noteActionRefs)
        {
            InputAction noteAction = noteActionRef.action;
            if (noteAction == null)
            {
                Debug.LogError($"noteAction is null for noteLine: {noteLine}");
                yield break;
            }

            noteAction.Disable();

            // 문자가 특수문자일 경우, 맵핑 사용
            if (specialKeyMappings.ContainsKey(newKey))
            {
                newKey = specialKeyMappings[newKey];
            }

            noteAction.AddBinding($"<Keyboard>/{newKey}");
            noteAction.Enable();
        }

        Debug.Log($"Add Note{noteLine} to <Keyboard>/{newKey}");
        yield break;
    }

    private InputActionReference[] FindNoteActionRef(int noteLine)
    {
        return noteLine switch
        {
            0 => note0ActionRefs,
            1 => note1ActionRefs,
            2 => note2ActionRefs,
            3 => note3ActionRefs,
            4 => note4ActionRefs,
            5 => note5ActionRefs,
            _ => null,
        };
    }
}