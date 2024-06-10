using UnityEngine;

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

    public RectTransform rect;
    public RectTransform dest;

    public int page = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        dest.anchoredPosition3D = new Vector3(1920f, 0f, 0f);
    }
}
