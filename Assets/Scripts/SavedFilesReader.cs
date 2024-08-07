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

public class SavedFileInfo
{
    public GameObject SavedFile { get; set; }
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

    private List<SavedFileInfo> savedFiles = new();
    private int currentKeyNumTabIdx = 0;
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

    public void ReadFiles()
    {
        ActivateKeyNumTab(currentKeyNumTabIdx);

        int keyNum = currentKeyNumTabIdx + 4;
        foreach (SavedFileInfo savedFileInfo in savedFiles)
        {
            Destroy(savedFileInfo.SavedFile);
        }
        savedFiles.Clear();

        string basePath = $"{Application.persistentDataPath}/Sheet/{keyNum}"; // 탐색할 루트 경로 설정
        DisplayDirectoryContents(basePath);
    }

    public void OnClickFourKeyTab()
    {
        currentKeyNumTabIdx = 0;
        ReadFiles();
    }
    public void OnClickFiveKeyTab()
    {
        currentKeyNumTabIdx = 1;
        ReadFiles();
    }
    public void OnClickSixKeyTab()
    {
        currentKeyNumTabIdx = 2;
        ReadFiles();
    }

    public void NextKeyNumTab(InputAction.CallbackContext context)
    {
        if (!Keyboard.current.shiftKey.isPressed)
        {
            if (++currentKeyNumTabIdx >= keyNumTabs.Length)
                currentKeyNumTabIdx = 0;

            ReadFiles();
        }
    }
    public void PrevKeyNumTab(InputAction.CallbackContext context)
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            if (--currentKeyNumTabIdx < 0)
                currentKeyNumTabIdx = keyNumTabs.Length - 1;

            ReadFiles();
        }
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

    public IEnumerator OnClickSavedFile(string path, string title)
    {
        yield return StartCoroutine(Parser.Instance.IEParseGameSheet(path, title));

        SheetLoader.Instance.isLoadFinish = true;
        GameManager.Instance.Title();
    }

    void DisplayDirectoryContents(string rootPath)
    {
        string[] directories = Directory.GetDirectories(rootPath);

        foreach (string directory in directories)
        {
            DisplayFiles(directory);
        }

        // 수정일 기준으로 내림차순 정렬
        savedFiles = savedFiles.OrderByDescending(info => info.LastModified).ToList();

        // 정렬된 리스트에 따라 UI를 업데이트
        foreach (SavedFileInfo fileInfo in savedFiles)
        {
            fileInfo.SavedFile.transform.SetSiblingIndex(savedFiles.IndexOf(fileInfo));
        }
    }

    private void DisplayFiles(string path)
    {
        DateTime sheetLastModified = DateTime.MinValue;

        // 세이브 파일 UI 인스턴스화
        GameObject savedFile = Instantiate(saveFilesPrefab, contentPanel);
        Transform Panel = savedFile.transform.GetChild(0);

        // 해당 경로의 모든 파일 로드
        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string extension = Path.GetExtension(file);

            if (extension == ".png")
            {
                string savedFilePath = Path.Combine(path, fileName);
                Image SavedThumbnail = Panel.GetChild(0).gameObject.GetComponent<Image>();

                SavedThumbnail.sprite = Parser.Instance.LoadImageFromLocal(savedFilePath);
            }
            else if (extension == ".sheet")
            {
                sheetLastModified = File.GetLastWriteTime(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                TextMeshProUGUI Title = Panel.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI LastModifiedText = Panel.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();

                Title.SetText(fileNameWithoutExtension);
                LastModifiedText.SetText($"마지막 수정일:\n{sheetLastModified:g}");

                Button fileButton = Panel.GetComponent<Button>();
                fileButton.onClick.AddListener(() => StartCoroutine(OnClickSavedFile(path, fileNameWithoutExtension)));

                // S3 업로드 여부 체크
                S3Uploader.Instance.CheckIfFileExists(fileNameWithoutExtension, currentKeyNumTabIdx + 4,
                    () =>
                    {
                        if (Panel == null) return;

                        GameObject Uploaded = Panel.GetChild(4).gameObject;
                        if (Uploaded != null)
                            Uploaded.SetActive(true);
                    },
                    () =>
                    {
                        if (Panel == null) return;

                        GameObject Uploaded = Panel.GetChild(4).gameObject;
                        if (Uploaded != null)
                            Uploaded.SetActive(false);
                    });
            }
        }

        Button deleteButton = Panel.GetChild(3).gameObject.GetComponent<Button>();
        deleteButton.onClick.AddListener(() => ReconfirmDeleteSheet(path));

        // 파일 정보를 객체에 추가
        savedFiles.Add(new SavedFileInfo
        {
            SavedFile = savedFile,
            LastModified = sheetLastModified
        });
    }

    private void ReconfirmDeleteSheet(string folderPath)
    {
        PopupController.Instance.InitByScene("SelectSheet",
            () => DeleteFolder(folderPath),
            () => PopupController.Instance.SetActiveCanvas(false));
        PopupController.Instance.SetActiveCanvas(true);
    }

    private void DeleteFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
            Debug.Log("Folder deleted successfully: " + folderPath);

            ReadFiles();
            PopupController.Instance.SetActiveCanvas(false);
        }
    }
}

#endif