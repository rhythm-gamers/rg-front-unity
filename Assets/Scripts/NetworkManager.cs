using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public IEnumerator GetRequest(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(www.downloadHandler.text);
            }
            else
            {
                onError?.Invoke(www.error);
            }
        }
    }
    public IEnumerator GetAudioRequest(string url, System.Action<AudioClip> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(DownloadHandlerAudioClip.GetContent(www));
            }
            else
            {
                onError?.Invoke(www.error);
            }
        }
    }
    public IEnumerator GetImgRequest(string url, System.Action<Texture2D> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(DownloadHandlerTexture.GetContent(www));
            }
            else
            {
                onError?.Invoke(www.error);
            }
        }
    }
}