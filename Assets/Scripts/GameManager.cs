using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputAction _resetAction;
    private InputAction _quitAction;

    void Awake()
    {
        _resetAction = InputSystem.actions.FindAction("Reset");
        _quitAction = InputSystem.actions.FindAction("Quit");
    }

    void Start()
    {
    }

    void Update()
    {
        if (_resetAction.triggered)
        {
            Reset();
        }

        if (_quitAction.triggered)
        {
            Application.Quit();
        }
    }

    void Reset()
    {
        GoalieController.Instance.ResetState();
        StrikerController.Instance.ResetState();
    }
}