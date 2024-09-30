#if !UNITY_WEBGL

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class S3Downloader : MonoBehaviour
{
    public GameObject popupPanel;

    public TMP_InputField titleInput;
    public TextMeshProUGUI errorLog;
    public Toggle[] checkboxes;

    public void TogglePopup()
    {
        popupPanel.SetActive(!popupPanel.activeSelf);
    }

    public void Download()
    {
        StartCoroutine(IEDownload());
    }

    private IEnumerator IEDownload()
    {
        string trimmedTitle = titleInput.text.Trim();
        if (trimmedTitle.Length == 0) yield break;

        bool isAllSuccess = true;
        bool isAllFailed = true;
        List<int> downloadFailedKeyNums = new();

        int waitingRequests = 0;
        for (int i = 0; i < checkboxes.Length; i++)
        {
            if (checkboxes[i].isOn)
            {
                int keyNum = 4 + i;
                string path = $"{EnvManager.Instance.CloudfrontUrl}/Sheet/{keyNum}/{trimmedTitle}";
                waitingRequests++;

                S3Uploader.Instance.CheckIfFileExists(trimmedTitle, keyNum,
                    () =>
                    {
                        StartCoroutine(SheetStorage.Instance.AddUploadedSheet(path, trimmedTitle, keyNum));
                        isAllFailed = false;
                        waitingRequests--;
                    },
                    () =>
                    {
                        downloadFailedKeyNums.Add(keyNum);
                        isAllSuccess = false;
                        waitingRequests--;
                    }
                );
            }
        }
        yield return new WaitUntil(() => waitingRequests == 0);

        if (isAllSuccess)
            errorLog.color = new Color(0f, 0f, 0f, 0f);
        else if (isAllFailed)
            errorLog.color = new Color(255 / 255f, 83 / 255f, 83 / 255f);
        else
            errorLog.color = new Color(249 / 255f, 217 / 217f, 35 / 255f);

        downloadFailedKeyNums.Sort();
        errorLog.text = $"{trimmedTitle}의 {string.Join(", ", downloadFailedKeyNums)}키 채보가 존재하지 않습니다.";
    }
}

#endif