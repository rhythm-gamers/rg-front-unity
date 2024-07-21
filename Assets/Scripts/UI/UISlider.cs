using UnityEngine.UI;

public class UISlider : UIObject
{
    public Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        //slider.onValueChanged.AddListener(OnValue);
    }

    public void OnValue(float value)
    {
        UIController.Instance.FindUI(Name);
    }
}
