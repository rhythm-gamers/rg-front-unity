using UnityEngine;
using TMPro;

public class UIText : UIObject
{
    TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string _text)
    {
        text.text = _text;
    }

    public void SetColor(Color color)
    {
        text.color = color;
    }

    public void ChangeText()
    {
        UIController.Instance.FindUI(Name);
    }
}
