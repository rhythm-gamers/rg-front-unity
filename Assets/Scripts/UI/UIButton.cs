using TMPro;
using UnityEngine.UI;

public class UIButton : UIObject
{
    Button btn;
    TextMeshProUGUI text;

    void Start()
    {
        btn = GetComponent<Button>();
        text = transform.GetComponentInChildren<TextMeshProUGUI>();
        btn.onClick.AddListener(OnClick);
    }

    public void SetText(string txt)
    {
        text.text = txt;
    }

    public void OnClick()
    {
        UIController.Instance.find.Invoke(Name);
    }
}
