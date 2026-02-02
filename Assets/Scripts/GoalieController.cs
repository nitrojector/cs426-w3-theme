using UnityEngine;
using UnityEngine.InputSystem;

public class GoalieController : MonoBehaviour
{
    public static GoalieController Instance { get; private set; }

    InputAction mvmtAction;

    private Vector3 _initialPosition;

    private float _position = 0.0f;
    private float _targetPosition = 0.0f;

    private float _abilityCooldown = 0.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _initialPosition = transform.position;
    }

    private void Start()
    {
        mvmtAction = InputSystem.actions.FindAction("MoveWASD");
        _position = transform.position.x;
        _targetPosition = _position;
    }

    private void Update()
    {
        Vector2 moveRaw = mvmtAction.ReadValue<Vector2>();
        float horMove = moveRaw.x;
        if (moveRaw != Vector2.zero)
        {
            Debug.Log("Move Input: " + moveRaw);
        }

        _targetPosition += horMove * 30.0f * Time.deltaTime;
        _targetPosition = Mathf.Clamp(_targetPosition, -Constants.PLAYER_HOR_MAX, Constants.PLAYER_HOR_MAX);
        _position = Mathf.Lerp(_position, _targetPosition, 16.0f * Time.deltaTime);
        transform.position = new Vector3(_position, transform.position.y, transform.position.z);
    }

    public void ResetState()
    {
        transform.position = _initialPosition;
        _position = transform.position.x;
        _targetPosition = _position;
        _abilityCooldown = 0.0f;
    }
}