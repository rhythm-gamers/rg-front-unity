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

    public Sheet originSheet = new();
    public bool bLoadFinish = false;

    public IEnumerator WebGLLoadSheet(string sheetName)
    {
        yield return Parser.Instance.IEParse(sheetName);
        yield return StartCoroutine(InitGameNotes());
        bLoadFinish = true;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
#if UNITY_EDITOR == true
        StartCoroutine(WebGLLoadSheet("Consolation"));
#endif

        InvokeRepeating(nameof(CheckElapsedTime), 0, 0.5f);
    }

    public IEnumerator InitGameNotes()
    {
        yield return GameManager.Instance.sheet = originSheet;
    }

    private void CheckElapsedTime()
    {
        if (bLoadFinish == true)
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
