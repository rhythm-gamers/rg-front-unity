using System.Collections;
using UnityEngine;

public class SheetLoader : MonoBehaviour
{
    static SheetLoader instance;
    public static SheetLoader Instance
    {
        get
        {
            return instance;
        }
    }

    readonly string basePath = EnvManager.Instance.CloudfrontUrl;

    public bool isLoadFinish = false;

    public IEnumerator WebGLLoadSheet(string sheetName)
    {
        yield return StartCoroutine(Parser.Instance.IEParseGameSheet($"{basePath}/Sheet/{sheetName}", sheetName));
        isLoadFinish = true;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
#if UNITY_WEBGL && UNITY_EDITOR
        StartCoroutine(WebGLLoadSheet("Consolation"));
#endif
        InvokeRepeating(nameof(CheckElapsedTime), 0, 0.5f);
    }

    private void CheckElapsedTime()
    {
        if (isLoadFinish == true)
        {
            CancelInvoke(nameof(CheckElapsedTime));
        }

        else if (Time.time > 10f)
        {
            CancelInvoke(nameof(CheckElapsedTime));
            Debug.Log("네트워크 오류");
        }
    }
}
