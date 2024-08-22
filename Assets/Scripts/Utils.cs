using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Utils : MonoBehaviour
{
    static Utils instance;
    public static Utils Instance
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

    public float MilliSecToBar(int milliSec)
    {
        float barPerMilliSec = GameManager.Instance.sheet.BarPerMilliSec;
        float barInterval = 16.0f; // == GridGenerator.Instance.barInterval

        return milliSec * (barInterval / barPerMilliSec);
    }

    public float CalculateStandardDeviationFromZero(List<int> values)
    {
        if (values.Count == 0) return 0f;

        float sumOfSquaresOfDifferences = values.Select(val => val * val).Sum();
        float variance = sumOfSquaresOfDifferences / values.Count;
        return Mathf.Sqrt(variance);
    }
}