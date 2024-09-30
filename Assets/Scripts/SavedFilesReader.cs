#if !UNITY_WEBGL

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Globalization;

public class SavedFileInfo
{
    public GameObject GameObject { get; set; }
    public DateTime LastModified { get; set; }
}

public class SavedFilesReader : MonoBehaviour
{
    static SavedFilesReader instance;
    public static SavedFilesReader Instance
    {
        get
        {
            return instance;
        }
    }

    public Transform contentPanel;
    public GameObject saveFilesPrefab;
    public GameObject[] keyNumTabs;
    public TextMeshProUGUI notFoundText;

    public bool isSavedFilesLoaded = true;
    public bool isFileChanged = true;

    private List<SavedFileInfo>[] savedFiles = { new(), new(), new() };
    private int currentTabIdx = 0;
    private InputActions inputActions;

    private InputAction nextKeyNumTabAction;
    private InputAction prevKeyNumTabAction;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            inputActions = new InputActions();

            nextKeyNumTabAction = inputActions.SelectSheet.NextKeyNumTab;
            nextKeyNumTabAction.performed += ctx => NextKeyNumTab(ctx);
            nextKeyNumTabAction.Enable();

            prevKeyNumTabAction = inputActions.SelectSheet.PrevKeyNumTab;
            prevKeyNumTabAction.performed += ctx => PrevKeyNumTab(ctx);
            prevKeyNumTabAction.Enable();
        }
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    public void OnClickFourKeyTab()
    {
        currentTabIdx = 0;
        ReadFiles();
    }
    public void OnClickFiveKeyTab()
    {
        currentTabIdx = 1;
        ReadFiles();
    }
    public void OnClickSixKeyTab()
    {
        currentTabIdx = 2;
        ReadFiles();
    }

    public void NextKeyNumTab(InputAction.CallbackContext context)
    {
        if (!Keyboard.current.shiftKey.isPressed)
        {
            if (++currentTabIdx >= keyNumTabs.Length)
                currentTabIdx = 0;

            ReadFiles();
        }
    }
    public void PrevKeyNumTab(InputAction.CallbackContext context)
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            if (--currentTabIdx < 0)
                currentTabIdx = keyNumTabs.Length - 1;
            ReadFiles();
        }
    }

    public void ReadFiles()
    {
        ActivateKeyNumTab(currentTabIdx);
        foreach (List<SavedFileInfo> SavedFiles in savedFiles)
        {
            foreach (SavedFileInfo SavedFile in SavedFiles)
            {
                SavedFile.GameObject.SetActive(false);
            }
        }

        int keyNum = currentTabIdx + 4;
        List<SavedFileInfo> currentTabFiles = savedFiles[currentTabIdx];

        if (currentTabFiles.Count == 0)
        {
            notFoundText.gameObject.SetActive(true);
        }
        else
        {
            notFoundText.gameObject.SetActive(false);

            // 수정일 기준으로 내림차순 정렬
            currentTabFiles = currentTabFiles.OrderByDescending(info => info.LastModified).ToList();
            savedFiles[currentTabIdx] = currentTabFiles;

            // 정렬된 리스트에 따라 UI를 업데이트
            for (int i = 0; i < currentTabFiles.Count; i++)
            {
                SavedFileInfo savedFile = currentTabFiles[i];
                savedFile.GameObject.transform.SetSiblingIndex(i);
                savedFile.GameObject.SetActive(true);
            }
        }
    }

    public async void AddSavedFileAtTab(string path, int keyNum)
    {
        string title = Path.GetFileName(path);
        List<SavedFileInfo> currentTab = savedFiles[keyNum - 4];

        SavedFileInfo TargetSavedFile = null;
        foreach (SavedFileInfo SavedFile in currentTab)
        {
            Transform panel = SavedFile.GameObject.transform.GetChild(0);
            string savedFileTitle = panel.GetChild(1).GetComponent<TextMeshProUGUI>().text;
            if (savedFileTitle == title)
            {
                TargetSavedFile = SavedFile;
                break;
            }
        }

        if (TargetSavedFile == null)
            await PreloadFiles(path, keyNum);
        else
        {
            Transform panel = TargetSavedFile.GameObject.transform.GetChild(0);
            TextMeshProUGUI SavedFileLastModified = panel.GetChild(2).GetComponent<TextMeshProUGUI>();

            DateTime sheetLastModified = File.GetLastWriteTime($"{path}/{title}.sheet");
            string lastModifiedText = $"마지막 수정일:\n{sheetLastModified.ToString("g", new CultureInfo("ko-KR"))}";

            SavedFileLastModified.text = lastModifiedText;
            TargetSavedFile.LastModified = sheetLastModified;
        }

        ReadFiles();
    }

    public async void PreloadSavedFilesAsync()
    {
        if (!isFileChanged) return;

        isSavedFilesLoaded = false;

        foreach (List<SavedFileInfo> SavedFiles in savedFiles)
        {
            foreach (SavedFileInfo SavedFile in SavedFiles)
            {
                Destroy(SavedFile.GameObject);
            }
        }
        savedFiles = new List<SavedFileInfo>[] { new(), new(), new() };

        foreach (int keyNum in new int[] { 4, 5, 6 })
        {
            string basePath = $"{Application.persistentDataPath}/Sheet/{keyNum}"; // 탐색할 루트 경로 설정
            await PreloadDirectoryContentsAsync(basePath, keyNum);
        }

        ReadFiles();
        isFileChanged = false;
        isSavedFilesLoaded = true;
    }

    private void ActivateKeyNumTab(int keyNumTabIdx)
    {
        foreach (GameObject keyNumTab in keyNumTabs)
        {
            Image tab = keyNumTab.GetComponent<Image>();
            Color tabColor = tab.color;
            tabColor.a = 100f / 255f;
            tab.color = tabColor;
        }

        Image currentTab = keyNumTabs[keyNumTabIdx].GetComponent<Image>();
        Color currentTabColor = currentTab.color;
        currentTabColor.a = 1;
        currentTab.color = currentTabColor;
    }

    private async Task PreloadDirectoryContentsAsync(string rootPath, int keyNum)
    {
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }

        string[] directories = Directory.GetDirectories(rootPath);
        foreach (string directory in directories)
        {
            await PreloadFiles(directory, keyNum);
        }
    }

    private async Task PreloadFiles(string path, int keyNum)
    {
        string currentFolderName = Path.GetFileName(path);
        DateTime sheetLastModified = DateTime.MinValue;

        // 세이브 파일 UI 인스턴스화
        GameObject savedFile = Instantiate(saveFilesPrefab, contentPanel);
        Transform Panel = savedFile.transform.GetChild(0);

        Image SavedThumbnail = Panel.GetChild(0).gameObject.GetComponent<Image>();
        TextMeshProUGUI Title = Panel.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI LastModifiedText = Panel.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();

        Button loadFileBtn = Panel.GetComponent<Button>();
        loadFileBtn.onClick.AddListener(() => StartCoroutine(OnClickSavedFile(path, currentFolderName)));

        // S3 업로드 여부 체크
        S3Uploader.Instance.CheckIfFileExists(currentFolderName, keyNum,
            () =>
            {
                GameObject Uploaded = Panel.GetChild(4).gameObject;
                Uploaded.SetActive(true);
            },
            () =>
            {
                GameObject Uploaded = Panel.GetChild(4).gameObject;
                Uploaded.SetActive(false);
            });

        byte[] savedThumbnailBytes = { };
        string title = currentFolderName;
        string lastModifiedText = "";

        await Task.Run(async () =>
        {
            // 해당 경로의 모든 파일 로드
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);

                if (extension == ".png")
                {
                    string savedFilePath = Path.Combine(path, fileName);
                    savedThumbnailBytes = await Parser.Instance.LoadImageFromLocalAsync(savedFilePath).ConfigureAwait(false);
                }
                else if (extension == ".sheet")
                {
                    sheetLastModified = File.GetLastWriteTime(file);
                    lastModifiedText = $"마지막 수정일:\n{sheetLastModified.ToString("g", new CultureInfo("ko-KR"))}";
                }
            }
        });

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(savedThumbnailBytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        SavedThumbnail.sprite = sprite;

        Title.SetText(title);
        LastModifiedText.SetText(lastModifiedText);

        SavedFileInfo savedFileInfo = new SavedFileInfo
        {
            GameObject = savedFile,
            LastModified = sheetLastModified
        };

        Button deleteButton = Panel.GetChild(3).gameObject.GetComponent<Button>();
        deleteButton.onClick.AddListener(() => ReconfirmDeleteSheet(path));

        savedFiles[keyNum - 4].Add(savedFileInfo);
    }

    private IEnumerator OnClickSavedFile(string path, string title)
    {
        yield return StartCoroutine(Parser.Instance.IEParseGameSheet(path, title));

        SheetLoader.Instance.isLoadFinish = true;
        GameManager.Instance.Title();
    }

    private void ReconfirmDeleteSheet(string folderPath)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        var raycastResults = new List<RaycastResult>();

        GraphicRaycaster raycaster = transform.GetComponent<GraphicRaycaster>();

        raycaster.Raycast(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            RaycastResult result = raycastResults[0];
            GameObject clickedObject = result.gameObject;
            int fileIdx = clickedObject.transform.parent.parent.parent.GetSiblingIndex();

            PopupController.Instance.InitByScene("SelectSheet",
               () => DeleteFolder(folderPath, fileIdx),
               () => PopupController.Instance.SetActiveCanvas(false));
            PopupController.Instance.SetActiveCanvas(true);
        }
    }

    private void DeleteFolder(string folderPath, int fileIdx)
    {
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
            Debug.Log("Folder deleted successfully: " + folderPath);

            List<SavedFileInfo> currentTabFiles = savedFiles[currentTabIdx];
            currentTabFiles = currentTabFiles.OrderByDescending(info => info.LastModified).ToList();

            Destroy(currentTabFiles[fileIdx].GameObject);
            currentTabFiles.RemoveAt(fileIdx);
            savedFiles[currentTabIdx] = currentTabFiles;

            ReadFiles();
            PopupController.Instance.SetActiveCanvas(false);
        }
    }
}

#endif