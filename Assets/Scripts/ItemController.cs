using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : MonoBehaviour
{
    static ItemController instance;
    public static ItemController Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject item;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        Image cover = item.transform.GetChild(0).GetComponent<Image>();
        TextMeshProUGUI level = item.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI title = item.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI artist = item.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();


        cover.sprite = GameManager.Instance.sheet.img;
        level.text = "";
        title.text = GameManager.Instance.sheet.title;
        artist.text = GameManager.Instance.sheet.artist;
    }
}
