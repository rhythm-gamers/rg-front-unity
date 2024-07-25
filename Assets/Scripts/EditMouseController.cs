using UnityEngine;
using UnityEngine.InputSystem;

public class EditMouseController : MonoBehaviour
{
    static EditMouseController instance;
    public static EditMouseController Instance
    {
        get
        {
            return instance;
        }
    }

    public Vector2 mousePos;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.Edit)
        {
            if (!GameManager.Instance.isPaused)
            {
                mousePos = Mouse.current.position.ReadValue();
            }
        }
    }
}