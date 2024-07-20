using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using System.Collections;
using UnityEngine.Networking;

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
    private string queryString;

    [System.Serializable]
    public class PresignedUrlResponse
    {
        public string url;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void UploadFile(string contentBody, string contentType)
    {
        Init();
        StartCoroutine(IEUploadFile(contentBody, contentType));
    }

    private void Init()
    {
        queryString = $"?bucketName={EnvManager.Instance.AWSBucketName}&objectKey=Sheet/{GameManager.Instance.sheet.title}/{GameManager.Instance.sheet.title}.sheet&expirationDuration=15";
    }

    private IEnumerator IEUploadFile(string contentBody, string contentType)
    {
        yield return StartCoroutine(IEGetPresignedUrl());

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(contentBody);
        using UnityWebRequest www = new UnityWebRequest(presignedUrl, "PUT")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        www.SetRequestHeader("Content-Type", contentType);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Editor.Instance.ShowProgressLog("File uploaded successfully.");
            Debug.Log("File uploaded successfully.");
        }
        else
        {
            Editor.Instance.ShowProgressLog("File upload failed: " + www.error);
            Debug.LogError("File upload failed: " + www.error);
        }
    }

    private IEnumerator IEGetPresignedUrl()
    {
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