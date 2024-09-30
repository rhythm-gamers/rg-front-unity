#if !UNITY_WEBGL

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.Xml.Linq;

public class S3Downloader : MonoBehaviour
{
    public GameObject savedFileFromCloudPrefab;
    public Transform contentPanel;
    public GameObject popupPanel;
    public GameObject[] keyNumTabs;
    public TextMeshProUGUI notFoundText;

    private List<string> filePathsAtKeyNum = new();
    private List<GameObject> savedFilesFromCloud = new();
    private List<string> selectedFilePaths = new();

    public void TogglePopup()
    {
        bool toggledState = !popupPanel.activeSelf;
        popupPanel.SetActive(toggledState);

        if (toggledState)
            ActivateKeyNumTab(4);
    }

    public IEnumerator IESearchSheetWithKeyNum(int keyNum)
    {
        filePathsAtKeyNum = new();

        string requestURL = $"{EnvManager.Instance.AWSGetAllUrl}?&prefix=Sheet/{keyNum}/&list-type=2&delimiter=/";
        using UnityWebRequest www = UnityWebRequest.Get(requestURL);

        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            XNamespace s3Namespace = "http://s3.amazonaws.com/doc/2006-03-01/";
            string xmlContent = www.downloadHandler.text;
            XDocument xmlDoc = XDocument.Parse(xmlContent);

            foreach (XElement commonPrefix in xmlDoc.Descendants(s3Namespace + "CommonPrefixes"))
            {
                string prefixValue = commonPrefix.Element(s3Namespace + "Prefix").Value;
                filePathsAtKeyNum.Add(prefixValue);
            }
        }
    }

    public void Download()
    {
        foreach (string path in selectedFilePaths)
        {
            int keyNum = int.Parse(path.Split("/")[1]);
            string title = path.Split("/")[2];
            string requestURL = $"{EnvManager.Instance.CloudfrontUrl}/Sheet/{keyNum}/{title}";

            S3Uploader.Instance.CheckIfFileExists(title, keyNum,
                () =>
                {
                    StartCoroutine(SheetStorage.Instance.AddUploadedSheet(requestURL, title, keyNum));
                },
                () => { }
            );
        }

        if (selectedFilePaths.Count > 0)
        {
            selectedFilePaths = new();
            TogglePopup();
        }
    }

    public void ActivateKeyNumTab(int keyNum)
    {
        StartCoroutine(IEActivateKeyNumTab(keyNum));
    }



    public void OnCheckboxValueChanged(bool isOn, string filePath)
    {
        if (isOn)
            selectedFilePaths.Add(filePath);
        else
        {
            int targetIdx = selectedFilePaths.IndexOf(filePath);
            selectedFilePaths.RemoveAt(targetIdx);
        }
    }

    private IEnumerator IEActivateKeyNumTab(int keyNum)
    {
        foreach (GameObject keyNumTab in keyNumTabs)
        {
            Image tab = keyNumTab.GetComponent<Image>();
            Color tabColor = tab.color;
            tabColor.a = 100f / 255f;
            tab.color = tabColor;
        }

        Image currentTab = keyNumTabs[keyNum - 4].GetComponent<Image>();
        Color currentTabColor = currentTab.color;
        currentTabColor.a = 1;
        currentTab.color = currentTabColor;

        yield return StartCoroutine(IESearchSheetWithKeyNum(keyNum));

        ReadFileNames();
    }

    private void ReadFileNames()
    {
        foreach (GameObject go in savedFilesFromCloud)
        {
            Destroy(go);
        }

        if (filePathsAtKeyNum.Count == 0)
            notFoundText.gameObject.SetActive(true);
        else
        {
            notFoundText.gameObject.SetActive(false);

            foreach (string path in filePathsAtKeyNum)
            {
                string fileName = path.Split("/")[2];
                GameObject savedFileFromCloud = Instantiate(savedFileFromCloudPrefab, contentPanel);

                Toggle checkbox = savedFileFromCloud.transform.GetChild(0).GetChild(0).GetComponent<Toggle>();
                TextMeshProUGUI fileNameUI = checkbox.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
                checkbox.isOn = selectedFilePaths.Contains(path);
                fileNameUI.text = fileName;

                checkbox.onValueChanged.AddListener((bool isOn) => OnCheckboxValueChanged(isOn, path));
                savedFilesFromCloud.Add(savedFileFromCloud);
            }
        }

    }
}

#endif