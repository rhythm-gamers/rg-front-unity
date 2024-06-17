using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PauseNavigator : MonoBehaviour
{

    static PauseNavigator instance;
    public static PauseNavigator Instance
    {
        get
        {
            return instance;
        }
    }

    // List to hold specific buttons
    public List<Button> buttons;
    private int currentIndex = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Navigate(int direction)
    {
        // Update the current index
        currentIndex += direction;

        // Ensure the index is within bounds
        if (currentIndex < 0)
        {
            currentIndex = buttons.Count - 1;
        }
        else if (currentIndex >= buttons.Count)
        {
            currentIndex = 0;
        }

        // Select the button
        EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
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
                UIController.Instance.find.Invoke(button.gameObject.name);
            }
        }
    }
}
