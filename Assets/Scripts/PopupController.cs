#if !UNITY_WEBGL

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    static PopupController instance;
    public static PopupController Instance
    {
        get
        {
            return instance;
        }
    }
    public Canvas popupCanvas;
    public GameObject Popup;

    private string Content { get; set; }
    private string BackText { get; set; }
    private string DangerText { get; set; }
    private UnityAction BackOnClick { get; set; }
    private UnityAction DangerOnClick { get; set; }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void SetActiveCanvas(bool active)
    {
        popupCanvas.gameObject.SetActive(active);
    }


    public void InitByScene(string canvasName, UnityAction dangerOnClick, UnityAction backOnClick)
    {
        switch (canvasName)
        {
            case "Editor":
                Content = "변경사항이 있습니다.\n에디터로 돌아가시겠습니까?";
                BackText = "예";
                BackOnClick = backOnClick;
                DangerText = "메뉴로";
                DangerOnClick = dangerOnClick;
                ApplyChanges();
                break;
            case "SelectSheet":
                Content = "정말로 삭제하시겠습니까?";
                BackText = "아니오";
                BackOnClick = backOnClick;
                DangerText = "삭제";
                DangerOnClick = dangerOnClick;
                ApplyChanges();
                break;
            default:
                Debug.Log("Canvas name does not exist in case");
                break;
        }
    }

    private void ApplyChanges()
    {
        TextMeshProUGUI content = Popup.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        Button yesButton = Popup.transform.GetChild(2).GetChild(0).GetComponent<Button>();
        Button noButton = Popup.transform.GetChild(2).GetChild(1).GetComponent<Button>();
        TextMeshProUGUI yesText = yesButton.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI noText = noButton.GetComponentInChildren<TextMeshProUGUI>();

        content.text = Content;
        yesText.text = BackText;
        noText.text = DangerText;
        yesButton.onClick.AddListener(BackOnClick);
        noButton.onClick.AddListener(DangerOnClick);
    }
}

#endif