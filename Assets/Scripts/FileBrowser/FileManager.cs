#if !UNITY_WEBGL

using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class FileManager : MonoBehaviour
{
    static FileManager instance;
    public static FileManager Instance
    {
        get
        {
            return instance;
        }
    }

    public AudioSource audioSource;
    public string audioPath;
    public Sprite thumbnail;
    public TMP_InputField titleInput;
    public TMP_InputField artistInput;
    public TMP_InputField bpmInput;
    public TMP_InputField signatureInput;

    public Button addSheetBtn;


    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        addSheetBtn.onClick.AddListener(OnAddSheetBtnClicked);
    }

    public void OnAddSheetBtnClicked()
    {
        Regex myRegExp = new(@"^[1-9]\d?/[1-9]\d?$");
        if (!myRegExp.IsMatch(signatureInput.text)) return;

        string[] s = signatureInput.text.Split('/');
        int[] signature = { int.Parse(s[0].Trim()), int.Parse(s[1].Trim()) };

        string title = titleInput.text.Trim();
        string artist = artistInput.text.Trim();
        int bpm = int.Parse(bpmInput.text);

        if (thumbnail == null) return;
        if (audioSource.clip == null) return;
        if (artist == "") return;
        if (title == "") return;
        if (bpm <= 0) return;

        Sheet newSheet = new()
        {
            artist = artist,
            title = title,
            bpm = bpm,
            signature = signature,
        };

        SheetStorage.Instance.AddNewSheet(newSheet, thumbnail, audioPath);

        GameManager.Instance.EditorMenu();
    }
}

#endif