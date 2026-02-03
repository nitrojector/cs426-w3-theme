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
        _material.color = !GameManager.Instance.GameState.FirstHalf ? Constants.YELLOW_COLOR : Constants.BLUE_COLOR;

        _targetPosition = Mathf.Clamp(_targetPosition, -Constants.PLAYER_HOR_MAX, Constants.PLAYER_HOR_MAX);
        _position = Mathf.Lerp(_position, _targetPosition, 16.0f * Time.deltaTime);
        transform.position = new Vector3(_position, transform.position.y, transform.position.z);

        ProcessInput();

        if (_holdingBall)
        {
            // Ensure the physics body doesn't get conflicting transform updates while held.
            if (_ballRb != null && !_ballRb.isKinematic)
                _ballRb.isKinematic = true;

            targetBall.transform.position = transform.position + Constants.HOLD_BALL_OFFSET;
        }
    }

    private void ProcessInput()
    {
        Vector2 moveRaw = (GameManager.Instance.GameState.FirstHalf ? firstHalfAction : secondHalfAction)
            .ReadValue<Vector2>();

        if (!GameManager.Instance.RoundStarted)
        {
            if (moveRaw != Vector2.zero && !GameManager.Instance.GameState.RoundEnd)
            {
                GameManager.Instance.RoundStarted = true;
            }
            else
            {
                return;
            }
        }

        float horMove = moveRaw.x;

        if (_holdingBall)
        {
            _targetPosition += horMove * 30.0f * Time.deltaTime;
        }
        else
        {
            float mindControlForce = 500.0f;

            if (_ballRb != null && !_ballRb.isKinematic)
                _ballRb.AddForce(Vector3.right * mindControlForce * horMove * Time.deltaTime);
        }

        if (moveRaw.y > 0.1f && _holdingBall)
        {
            _holdingBall = false;
            // Make sure the physics body will respond to forces
            if (_ballRb != null)
            {
                // Align rigidbody / transform and clear motion to avoid teleport impulses
                var releasePos = transform.position + Constants.HOLD_BALL_OFFSET;
                targetBall.transform.position = releasePos;
                _ballRb.position = releasePos;
                _ballRb.linearVelocity = Vector3.zero;
                _ballRb.angularVelocity = Vector3.zero;
                _ballRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                _ballRb.isKinematic = false;
            }

            StartCoroutine(ReleaseAndShoot());
        }
    }

    private System.Collections.IEnumerator ReleaseAndShoot()
    {
        // Wait one physics step to let the physics engine register the rigidbody state change
        yield return new WaitForFixedUpdate();
        Shoot();
    }

    public void Shoot()
    {
        Vector3 shootDir = new Vector3(0.0f, 0.0f, 1.0f).normalized;
        float shootImpulse = 20.0f; // tuned impulse magnitude (Newton-seconds)
        if (_ballRb != null)
            _ballRb.AddForce(shootDir * shootImpulse, ForceMode.Impulse);
    }

    public void ResetState()
    {
        ResetBallToStriker();
        _targetPosition = 0.0f;
        _position = 0.0f;
    }

    public void ResetBallToStriker()
    {
        if (_ballRb != null)
        {
            _ballRb.angularVelocity = Vector3.zero;
            _ballRb.linearVelocity = Vector3.zero;
            _ballRb.rotation = Quaternion.identity;
            _ballRb.isKinematic = true;
        }

        if (targetBall != null)
            targetBall.transform.position = transform.position + Constants.HOLD_BALL_OFFSET;

        _holdingBall = true;
    }
}