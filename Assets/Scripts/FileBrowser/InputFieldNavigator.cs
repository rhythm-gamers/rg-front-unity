using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputFieldNavigator : MonoBehaviour
{
    public TMP_InputField[] inputFields;

    private InputAction navigateAction;
    private int currentIndex = 0;

    void Awake()
    {
        InputActionAsset inputActionAsset = Resources.Load<InputActionAsset>("Controller");
        navigateAction = inputActionAsset.FindActionMap("Navigator").FindAction("NextInputField");

        navigateAction.performed += ctx => OnNavigate(ctx);

        navigateAction.Enable();

        if (inputFields.Length > 0)
        {
            inputFields[0].ActivateInputField();
        }
    }

    void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.control.device is Keyboard && (context.control.name == "enter" || context.control.name == "numpadEnter" || context.control.name == "tab"))
        {
            currentIndex++;
            if (currentIndex >= inputFields.Length)
            {
                currentIndex = 0; // 마지막 InputField에서 엔터를 누르면 첫 번째 InputField로 돌아갑니다.
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