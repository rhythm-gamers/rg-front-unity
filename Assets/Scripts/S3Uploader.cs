#if !UNITY_WEBGL

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.IO;

public class S3Uploader : MonoBehaviour
{
    static S3Uploader instance;
    public static S3Uploader Instance
    {
        get
        {
            return instance;
        }
    }

    private string presignedUrl;

    [Serializable]
    public class PresignedUrlResponse
    {
        public string url;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void UploadSheet(string localFilePath, string title, int keyNum)
    {
        StartCoroutine(IEUploadSheet($"{localFilePath}/{keyNum}/{title}/{title}.sheet", title, keyNum));
        StartCoroutine(IEUploadImage($"{localFilePath}/{keyNum}/{title}/{title}.png", title, keyNum));
        StartCoroutine(IEUploadMp3($"{localFilePath}/{keyNum}/{title}/{title}.mp3", title, keyNum));
    }

    public void CheckIfFileExists(string title, int keyNum, Action onSuccess, Action onFail)
    {
        StartCoroutine(IECheckIfFileExists(title, keyNum, onSuccess, onFail));
    }


    private IEnumerator IEUploadSheet(string localFilePath, string title, int keyNum)
    {
        yield return StartCoroutine(IEGetPresignedUrl(title, keyNum, ".sheet", "put"));

        byte[] bodyRaw;
        try
        {
            bodyRaw = File.ReadAllBytes(localFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read file: " + e.Message);
            yield break;
        }

        using UnityWebRequest www = new UnityWebRequest(presignedUrl, "PUT")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        www.SetRequestHeader("Content-Type", "binary/octet-stream");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Editor.Instance.ShowProgressLog("Sheet uploaded successfully.");
            Debug.Log("Sheet uploaded successfully.");
        }
        else
        {
            Editor.Instance.ShowProgressLog("Sheet upload failed: " + www.error);
            Debug.LogError("Sheet upload failed: " + www.error);
        }
    }

    private IEnumerator IEUploadImage(string filePath, string title, int keyNum)
    {
        yield return StartCoroutine(IEGetPresignedUrl(title, keyNum, ".png", "put"));

        byte[] fileData;
        try
        {
            fileData = File.ReadAllBytes(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read file: " + e.Message);
            yield break;
        }

        using UnityWebRequest www = new UnityWebRequest(presignedUrl, "PUT")
        {
            uploadHandler = new UploadHandlerRaw(fileData),
            downloadHandler = new DownloadHandlerBuffer()
        };
        www.SetRequestHeader("Content-Type", "image/png");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Editor.Instance.ShowProgressLog("File uploaded successfully.");
            Debug.Log("Image uploaded successfully.");
        }
        else
        {
            Editor.Instance.ShowProgressLog("File upload failed: " + www.error);
            Debug.LogError("Image upload failed: " + www.error);
        }
    }

    private IEnumerator IEUploadMp3(string filePath, string title, int keyNum)
    {
        yield return StartCoroutine(IEGetPresignedUrl(title, keyNum, ".mp3", "put"));

        byte[] fileData;
        try
        {
            fileData = File.ReadAllBytes(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read file: " + e.Message);
            yield break;
        }

        using UnityWebRequest www = new UnityWebRequest(presignedUrl, "PUT")
        {
            uploadHandler = new UploadHandlerRaw(fileData),
            downloadHandler = new DownloadHandlerBuffer()
        };
        www.SetRequestHeader("Content-Type", "audio/mpeg");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Mp3 uploaded successfully.");
        }
        else
        {
            Debug.LogError("Mp3 upload failed: " + www.error);
        }
    }

    private IEnumerator IECheckIfFileExists(string title, int keyNum, Action onSuccess, Action onFail)
    {
        yield return StartCoroutine(IEGetPresignedUrl(title, keyNum, ".sheet", "head"));

        UnityWebRequest www = UnityWebRequest.Head(presignedUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke();
        else
            onFail?.Invoke();
    }


    private IEnumerator IEGetPresignedUrl(string title, int keyNum, string extension, string method)
    {
        string queryString = $"?bucketName={EnvManager.Instance.AWSBucketName}&objectKey=Sheet/{keyNum}/{title}/{title}{extension}&expirationDuration=15&method={method}";
        using UnityWebRequest www = UnityWebRequest.Get(EnvManager.Instance.AWSGenPresignedUrl + queryString);

        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = www.downloadHandler.text;

            PresignedUrlResponse response = JsonUtility.FromJson<PresignedUrlResponse>(jsonResponse);
            presignedUrl = response.url;
        }
        else
        {
            Editor.Instance.ShowProgressLog("IEGetPresignedUrl Error: " + www.error);
            Debug.LogError("IEGetPresignedUrl Error: " + www.error);
        }
    }
}

#endif