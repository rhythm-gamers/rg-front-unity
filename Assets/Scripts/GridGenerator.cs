#if !UNITY_WEBGL

using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    static GridGenerator instance;
    public static GridGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject grid;

    public readonly int barInterval = 16;
    public int lineCount = 192; // 3배수 비트를 지원하기위해 64개가 아닌 192개의 라인 설정
    public int barCount;

    private int gridOffsetUnit = 1; // 16비트 단위로 그리드를 움직임 (1 bar distance = 16)

    List<GameObject> gridList = new List<GameObject>();

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        int barPerMilliSec = GameManager.Instance.sheet.BarPerMilliSec;
        float gridOffset = Utils.Instance.MilliSecToBar(GameManager.Instance.sheet.offset);

        barCount = (int)(AudioManager.Instance.Length * 1000 / barPerMilliSec);

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
            obj.transform.localPosition = new Vector3(0f, (i * barInterval) + gridOffset);
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

    public void GridOffsetUp()
    {
        int offset = GameManager.Instance.sheet.offset;
        int barPerMilliSec = GameManager.Instance.sheet.BarPerMilliSec;

        if (offset >= AudioManager.Instance.Length * 1000)
            GameManager.Instance.sheet.offset = (int)(AudioManager.Instance.Length * 1000);
        else
            GameManager.Instance.sheet.offset += barPerMilliSec / barInterval;

        MoveGridOffset(gridOffsetUnit);
    }

    public void GridOffsetDown()
    {
        int offset = GameManager.Instance.sheet.offset;
        int barPerMilliSec = GameManager.Instance.sheet.BarPerMilliSec;

        if (offset <= 0)
            GameManager.Instance.sheet.offset = 0;
        else
            GameManager.Instance.sheet.offset -= barPerMilliSec / barInterval;

        MoveGridOffset(-gridOffsetUnit);
    }

    private void MoveGridOffset(int gridOffsetUnit)
    {
        foreach (GameObject grid in gridList)
        {
            Vector3 gridPos = grid.transform.localPosition;
            gridPos.y += gridOffsetUnit;

            grid.transform.localPosition = gridPos;
        }
    }
}

#endif