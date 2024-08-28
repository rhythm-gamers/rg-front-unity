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

    public float CalculatePredictionInterval(List<int> values)
    {
        float confidenceLevel = 1.96f; // 신뢰구간 95%
        float average = (float)values.Average();

        float sumOfSquaresOfDifferences = values.Select(val => Mathf.Pow(val - average, 2)).Sum();
        float standardDeviation = Mathf.Sqrt(sumOfSquaresOfDifferences / (values.Count - 1));
        float variance = Mathf.Pow(standardDeviation, 2);

        float standardError = standardDeviation / Mathf.Sqrt(values.Count);
        float predictionInterval = confidenceLevel * Mathf.Sqrt(variance + standardError * standardError);

        return predictionInterval;
    }
}