using UnityEngine;
using UnityEngine.UI;

public class UIScaler : MonoBehaviour
{
    public CanvasScaler canvasScaler;

    void Start()
    {
        AdjustCanvasScale();
    }

    void AdjustCanvasScale()
    {
        float targetAspect = 16.0f / 9.0f;
        float windowAspect = Screen.width / (float)Screen.height;

        // Adjust the canvas scaler to match the target aspect ratio
        if (windowAspect >= targetAspect)
        {
            canvasScaler.matchWidthOrHeight = 0.0f; // Match height
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 1.0f; // Match width
        }
    }
}