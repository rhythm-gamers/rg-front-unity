#if !UNITY_WEBGL

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleFileBrowser;
using System.IO;
using TMPro;

public class MusicLoader : MonoBehaviour
{
    public Button loadButton;
    public TextMeshProUGUI displaySongName;

    public AudioSource audioSource;

    void Start()
    {
        // Set up the button click event
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        audioSource = GetComponent<AudioSource>();
    }

    public void Init()
    {
        if (audioSource != null)
        {
            audioSource.clip = null;
            displaySongName.text = "음원 이름.mp3";
        }
    }

    public void Play()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    public void Stop()
    {
        if (audioSource.clip != null)
        {
            audioSource.Stop();
        }
    }


    void OnLoadButtonClicked()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Audios", ".mp3"));
        FileBrowser.SetDefaultFilter(".mp3");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".pem", ".ini", ".txt", ".log", ".pub", ".msi", ".mpx", ".doc", ".docx", ".mp4", ".pptx", ".pdf", ".json", ".url", ".hwp", ".hwpx");

        FileBrowser.ShowLoadDialog(OnFileSelected, OnFileSelectionCanceled, FileBrowser.PickMode.Files, false, null, null, "Select Music File", "Select");
    }

    void OnFileSelected(string[] paths)
    {
        if (paths.Length > 0)
        {
            if (audioSource.clip != null)
                Stop();

            string filePath = paths[0];
            string fileName = Path.GetFileName(filePath);

            displaySongName.SetText(fileName);

            StartCoroutine(LoadMusic(filePath));
        }
    }

    void OnFileSelectionCanceled()
    {
        Debug.Log("File selection canceled.");
    }

    public IEnumerator LoadMusic(string filePath)
    {
        AudioClip localClip = null;

        yield return StartCoroutine(NetworkManager.Instance.GetAudioRequest(filePath,
                data =>
                {
                    localClip = data;
                },
                error =>
                {
                    Debug.LogError($"Failed to download local clip: {error}");
                }
            ));

        audioSource.clip = localClip;

        FileManager.Instance.audioPath = filePath;
        yield return null;
    }
}

#endif