using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputFieldNavigator : MonoBehaviour
{
    public TMP_InputField[] inputFields;

    private InputAction navigateAction;
    private int currentIndex = 0;

    private InputActions inputActions;

    void Awake()
    {
        inputActions = new InputActions();
        navigateAction = inputActions.Navigator.NextInputField;

        navigateAction.performed += ctx => OnNavigate(ctx);

        navigateAction.Enable();

        if (inputFields.Length > 0)
        {
            inputFields[0].ActivateInputField();
        }
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.control.device is Keyboard && (context.control.name == "enter" || context.control.name == "numpadEnter" || context.control.name == "tab"))
        {
            currentIndex++;
            if (currentIndex >= inputFields.Length)
            {
                currentIndex = 0;
            }

            inputFields[currentIndex].ActivateInputField();
        }
    }

    void OnDestroy()
    {
        // navigateAction 비활성화
        navigateAction.Disable();
        navigateAction.performed -= ctx => OnNavigate(ctx);
    }
}