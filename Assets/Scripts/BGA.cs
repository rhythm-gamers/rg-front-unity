using UnityEngine;

public class BGA : MonoBehaviour
{
    public RectTransform canvasRectTransform;

    public void Init()
    {
        UIImage uiBGA = UIController.Instance.FindUI("UI_B_BGA").uiObject as UIImage;
        uiBGA.SetSprite(GameManager.Instance.sheet.img);
    }
}
