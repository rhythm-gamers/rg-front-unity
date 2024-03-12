using System;

public class UIActor
{
    public UIObject uiObject;
    public Action<UIObject> action;

    public UIActor(UIObject uiObject, Action<UIObject> action = null)
    {
        this.uiObject = uiObject;
        this.action = action;
    }
}
