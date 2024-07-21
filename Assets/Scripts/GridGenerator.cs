using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject grid;

    public readonly int barInterval = 16;
    public int barCount;
    public int lineCount = 192; // 3배수 비트를 지원하기위해 64개가 아닌 192개의 라인 설정

    List<GameObject> gridList = new List<GameObject>();

    public void Init()
    {
        barCount = (int)(AudioManager.Instance.Length * 1000 / GameManager.Instance.sheet.BarPerMilliSec);

        if (gridList.Count < barCount)
        {
            for (int i = gridList.Count; i < barCount; i++)
            {
                GameObject obj = Instantiate(grid, transform);
                obj.SetActive(false);
                gridList.Add(obj);
            }
        }

        for (int i = 0; i < barCount; i++)
        {
            GameObject obj = gridList[i];
            obj.name = $"Grid_{i}";
            obj.GetComponent<GridObject>().index = i;
            obj.transform.localPosition = Vector3.up * i * barInterval;
            obj.SetActive(true);
        }
    }

    public void InActivate()
    {
        foreach (GameObject obj in gridList)
        {
            obj.SetActive(false);
        }
    }
}
