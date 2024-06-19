using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RebindController : MonoBehaviour
{
    public InputActionReference note0ActionRef;
    public InputActionReference note1ActionRef;
    public InputActionReference note2ActionRef;
    public InputActionReference note3ActionRef;

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

    public void WebGLRebindNoteKey(string combinedArgs)
    {
        var args = combinedArgs.Split(',');
        int.TryParse(args[0], out int noteLine);
        string newKey = args[1];

        StartCoroutine(IERebindNoteAction(noteLine, newKey));
    }

    private IEnumerator IERebindNoteAction(int noteLine, string newKey)
    {
        InputActionReference noteActionRef = FindNoteActionRef(noteLine);
        if (noteActionRef == null)
        {
            Debug.LogError($"noteActionRef is null for noteLine: {noteLine}");
            yield break;
        }

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

        noteAction.ApplyBindingOverride(0, $"<Keyboard>/{newKey}");
        Debug.Log($"Rebound {noteAction.name} to <Keyboard>/{newKey}");

        noteAction.Enable();

        // Save the new binding
        SaveBindings(noteActionRef, noteLine);

        yield break;
    }

    private void SaveBindings(InputActionReference inputActionRef, int noteLine)
    {
        var rebinds = inputActionRef.action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString($"NoteLine{noteLine}", rebinds);
        PlayerPrefs.Save();
    }

    private InputActionReference FindNoteActionRef(int noteLine)
    {
        return noteLine switch
        {
            0 => note0ActionRef,
            1 => note1ActionRef,
            2 => note2ActionRef,
            3 => note3ActionRef,
            _ => null,
        };
    }
}