using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputFieldNavigator : MonoBehaviour
{
    public TMP_InputField[] inputFields;

    private InputAction nextInputFieldAction;
    private InputAction prevInputFieldAction;
    private int currentIndex = 0;

    private InputActions inputActions;

    void Awake()
    {
        inputActions = new InputActions();

        nextInputFieldAction = inputActions.Navigator.NextInputField;
        nextInputFieldAction.performed += ctx => NavigateToNextInputField(ctx);
        nextInputFieldAction.Enable();

        prevInputFieldAction = inputActions.Navigator.PrevInputField;
        prevInputFieldAction.performed += ctx => NavigateToPrevInputField(ctx);
        prevInputFieldAction.Enable();

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

    void NavigateToNextInputField(InputAction.CallbackContext context)
    {
        if (!Keyboard.current.shiftKey.isPressed)
        {
            if (++currentIndex >= inputFields.Length)
                currentIndex = 0;

            inputFields[currentIndex].ActivateInputField();
        }
    }

    void NavigateToPrevInputField(InputAction.CallbackContext context)
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            if (--currentIndex < 0)
                currentIndex = inputFields.Length - 1;

            inputFields[currentIndex].ActivateInputField();
        }
    }

    void OnDestroy()
    {
        nextInputFieldAction.Disable();
        nextInputFieldAction.performed -= ctx => NavigateToNextInputField(ctx);

        prevInputFieldAction.Disable();
        prevInputFieldAction.performed -= ctx => NavigateToPrevInputField(ctx);
    }
}