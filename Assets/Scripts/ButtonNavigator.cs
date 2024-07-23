using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonNavigator : MonoBehaviour
{
    private int currentIndex = 0;

    void OnEnable()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isPlaying)
        {
            currentIndex = 0;
            GameManager.Instance.isPlaying = false;
            GameManager.Instance.isPaused = true;
            EventSystem.current.SetSelectedGameObject(transform.GetChild(currentIndex).gameObject);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.isPlaying = true;
        GameManager.Instance.isPaused = false;
    }

    public void Navigate(int direction)
    {
        // Update the current index
        currentIndex += direction;

        // Ensure the index is within bounds
        if (currentIndex < 0)
        {
            currentIndex = transform.childCount - 1;
        }
        else if (currentIndex >= transform.childCount)
        {
            currentIndex = 0;
        }

        // Select the button
        EventSystem.current.SetSelectedGameObject(transform.GetChild(currentIndex).gameObject);
    }

    public void ActivateButton()
    {
        // Get the current selected GameObject
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject != null)
        {
            // Check if it has a Button component and invoke its onClick event
            Button button = selectedObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
            }
        }
    }

    public void OnArrowDownRight()
    {
        Navigate(1);
    }
}
