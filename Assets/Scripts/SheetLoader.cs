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

    public string pathSheet = "https://drt2kw8kpttus.cloudfront.net";

    public bool bLoadFinish = false;

    IEnumerator WebGLLoadSheet(string sheetName)
    {
        yield return Parser.Instance.IEParse(sheetName);
        bLoadFinish = true;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        InvokeRepeating(nameof(CheckElapsedTime), 0, 0.5f);
    }

    private void CheckElapsedTime()
    {
        if (bLoadFinish == true)
            CancelInvoke(nameof(CheckElapsedTime));

        else if (Time.time > 10f)
        {
            CancelInvoke(nameof(CheckElapsedTime));
            Debug.Log("네트워크 오류");
        }
    }
}
