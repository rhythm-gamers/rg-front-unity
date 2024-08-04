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
}