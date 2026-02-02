using UnityEngine;
using UnityEngine.InputSystem;

public class GoalieController : MonoBehaviour
{
    public static GoalieController Instance { get; private set; }

    InputAction firstHalfAction;
    InputAction secondHalfAction;

    private Vector3 _initialPosition;

    private float _position = 0.0f;
    private float _targetPosition = 0.0f;

    private float _abilityCooldown = 0.0f;

    private Material _material;

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
        _material = GetComponent<Renderer>().material;
    }

    private void Start()
    {
        firstHalfAction = InputSystem.actions.FindAction("MoveWASD");
        secondHalfAction = InputSystem.actions.FindAction("MoveArrow");
        _position = transform.position.x;
        _targetPosition = _position;
    }

    private void Update()
    {
        Vector2 moveRaw = (GameManager.Instance.GameState.FirstHalf ? firstHalfAction : secondHalfAction)
            .ReadValue<Vector2>();
        float horMove = moveRaw.x;
        if (moveRaw != Vector2.zero)
        {
            Debug.Log("Move Input: " + moveRaw);
        }

        _targetPosition += horMove * 30.0f * Time.deltaTime;
        _targetPosition = Mathf.Clamp(_targetPosition, -Constants.PLAYER_HOR_MAX, Constants.PLAYER_HOR_MAX);
        _position = Mathf.Lerp(_position, _targetPosition, 16.0f * Time.deltaTime);
        transform.position = new Vector3(_position, transform.position.y, transform.position.z);
        _material.color = GameManager.Instance.GameState.FirstHalf ? Constants.YELLOW_COLOR : Constants.BLUE_COLOR;
    }

    public void ResetState()
    {
        transform.position = _initialPosition;
        _position = transform.position.x;
        _targetPosition = _position;
        _abilityCooldown = 0.0f;
    }
}