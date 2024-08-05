#if !UNITY_WEBGL

using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using TMPro;

public class ImageLoader : MonoBehaviour
{
    public Button loadButton;
    public Image displayImage;
    public TextMeshProUGUI guideText;

    void Start()
    {
        loadButton.onClick.AddListener(OnLoadButtonClicked);
    }

    public void Init()
    {
        if (displayImage != null)
        {
            displayImage.sprite = null;
            displayImage.enabled = true;

            Color color = guideText.color;
            color.a = 255;
            guideText.color = color;
        }
    }

    void OnLoadButtonClicked()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"));
        FileBrowser.SetDefaultFilter(".png");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".pem", ".ini", ".txt", ".log", ".pub", ".msi", ".mpx", ".doc", ".docx", ".mp4", ".pptx", ".mp3", ".pdf", ".json", ".url", ".hwp", ".hwpx");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        FileBrowser.ShowLoadDialog(OnFileSelected, OnFileSelectionCanceled, FileBrowser.PickMode.Files, false, null, null, "Select Image", "Select");
    }

    void OnFileSelected(string[] paths)
    {
        if (paths.Length > 0)
        {
            string filePath = paths[0];
            Sprite sprite = Parser.Instance.LoadImageFromLocal(filePath);

            Color color = guideText.color;
            color.a = 0;
            guideText.color = color;

            displayImage.sprite = sprite;
        }
    }

    void OnFileSelectionCanceled()
    {
        Debug.Log("File selection canceled.");
    }
}

#endif
