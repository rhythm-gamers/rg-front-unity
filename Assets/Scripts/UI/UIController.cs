using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    static UIController instance;
    public static UIController Instance
    {
        get { return instance; }
    }

    public bool isInit = false;
    Dictionary<string, UIActor> uiObjectDic = new Dictionary<string, UIActor>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        UIObject[] objs = FindObjectsOfType<UIObject>();
        foreach (UIObject obj in objs)
        {
            uiObjectDic.Add(obj.Name, new UIActor(obj, null));
        }

        uiObjectDic["UI_G_Judgement"].action = Score.Instance.Ani;
        uiObjectDic["UI_G_Combo"].action = Score.Instance.Ani;

#if !UNITY_WEBGL
        uiObjectDic["UI_E_Play"].action = Editor.Instance.Play;
        uiObjectDic["UI_E_Stop"].action = Editor.Instance.Stop;
#endif

        isInit = true;
    }

    public UIActor FindUI(string uiName)
    {
        UIActor actor = uiObjectDic[uiName];
        if (actor.action != null)
            actor.action.Invoke(actor.uiObject);
        return actor;
    }

    public UIActor GetUI(string uiName)
    {
        UIActor actor = uiObjectDic[uiName];
        return actor;
    }
}
