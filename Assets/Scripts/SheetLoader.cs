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
    public int sheetCount = 2;
    public bool bLoadFinish;
    int remain;

    public string[] sheetNames = { "Consolation", "Splendid Circus" };

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {

        remain = sheetCount;
        StartCoroutine(IELoad());
    }

    IEnumerator IELoad()
    {
        foreach (string f_name in sheetNames)
        {
            yield return Parser.Instance.IEParse(f_name);
            GameManager.Instance.sheets.Add(f_name, Parser.Instance.sheet);
            if (--remain <= 0)
                bLoadFinish = true;
        }
    }
}
