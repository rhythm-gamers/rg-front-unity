using System;
using UnityEngine;
using UnityEngine.UI;

public class UIImage : UIObject
{
    Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite, Action<Image, Sprite> scalerFunc = null)
    {
        scalerFunc?.Invoke(image, sprite);
        image.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }
}
