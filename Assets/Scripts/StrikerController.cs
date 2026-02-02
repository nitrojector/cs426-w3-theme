using UnityEngine;
using UnityEngine.InputSystem;

public class StrikerController : MonoBehaviour
{
    public static StrikerController Instance { get; private set; }

    private Vector3 _initialPosition;

    InputAction firstHalfAction;
    InputAction secondHalfAction;

    private float _position = 0.0f;
    private float _targetPosition = 0.0f;

    private bool _holdingBall = true;

    [SerializeField] private GameObject targetBall = null;
    private Rigidbody _ballRb = null;

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

        firstHalfAction = InputSystem.actions.FindAction("MoveArrow");
        secondHalfAction = InputSystem.actions.FindAction("MoveWASD");
        _position = transform.position.x;
        _targetPosition = _position;
        _ballRb = targetBall.GetComponent<Rigidbody>();

        _material = GetComponent<Renderer>().material;
    }

    private void Start()
    {
    }

    private void Update()
    {
        _targetPosition = Mathf.Clamp(_targetPosition, -Constants.PLAYER_HOR_MAX, Constants.PLAYER_HOR_MAX);
        _position = Mathf.Lerp(_position, _targetPosition, 16.0f * Time.deltaTime);
        transform.position = new Vector3(_position, transform.position.y, transform.position.z);

        _material.color = !GameManager.Instance.GameState.FirstHalf ? Constants.YELLOW_COLOR : Constants.BLUE_COLOR;

        ProcessInput();

        if (_holdingBall)
        {
            targetBall.transform.position = transform.position + Constants.HOLD_BALL_OFFSET;
        }
    }

    private void ProcessInput()
    {
        Vector2 moveRaw = (GameManager.Instance.GameState.FirstHalf ? firstHalfAction : secondHalfAction)
            .ReadValue<Vector2>();
        float horMove = moveRaw.x;
        if (moveRaw != Vector2.zero)
        {
            Debug.Log("Move Input: " + moveRaw);
        }

        if (_holdingBall)
        {
            _targetPosition += horMove * 30.0f * Time.deltaTime;
        }
        else
        {
            float mindControlForce = 500.0f;

            _ballRb.AddForce(Vector3.right * mindControlForce * horMove * Time.deltaTime);
            // TODO: maybe lerp x-position instead of using force
        }

        if (moveRaw.y > 0.1f && _holdingBall)
        {
            _holdingBall = false;
            Shoot();
        }
    }

    public void Shoot()
    {
        Vector3 shootDir = new Vector3(0.0f, 0.0f, 1.0f).normalized;
        float shootForce = 800.0f;
        _ballRb.AddForce(shootDir * shootForce);
    }

    public void ResetState()
    {
        ResetBallToStriker();
        transform.position = _initialPosition;
    }

    public void ResetBallToStriker()
    {
        _ballRb.angularVelocity = Vector3.zero;
        _ballRb.linearVelocity = Vector3.zero;
        _ballRb.rotation = Quaternion.identity;
        _holdingBall = true;
    }
}