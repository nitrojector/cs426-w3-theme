using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    // TODO: we are not yet progressing halves or rounds

    public static GameManager Instance { get; private set; }

    private UIDocument _uiDocument;
    private VisualElement _rootEl;
    private Label _blueScoreLabel;
    private VisualElement _blueScoreContainer;
    private Label _yellowScoreLabel;
    private VisualElement _yellowScoreContainer;

    // classes: circle-green, circle-red

    private InputAction _resetAction;
    private InputAction _quitAction;
    private InputAction _toggleHalfAction;
    public readonly GameState GameState = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _resetAction = InputSystem.actions.FindAction("Reset");
        _quitAction = InputSystem.actions.FindAction("Quit");
        _toggleHalfAction = InputSystem.actions.FindAction("ToggleHalf");
        _toggleHalfAction.started += (ctx) => { GameState.FirstHalf = !GameState.FirstHalf; };
        _uiDocument = GetComponent<UIDocument>();
    }

    void Start()
    {
        _rootEl = GetComponent<UIDocument>().rootVisualElement;
        _blueScoreLabel = _rootEl.Q<Label>("blue-score");
        _blueScoreContainer = _rootEl.Q<VisualElement>("blue-scorebox");
        _yellowScoreLabel = _rootEl.Q<Label>("yellow-score");
        _yellowScoreContainer = _rootEl.Q<VisualElement>("yellow-scorebox");
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

        _blueScoreLabel.text = GameState.ScoreBlue.ToString();
        _yellowScoreLabel.text = GameState.ScoreYellow.ToString();
    }

    public void ScoreBlue()
    {
        GameState.ScoreBlue += 1;
    }

    public void ScoreYellow()
    {
        GameState.ScoreYellow += 1;
    }

    void Reset()
    {
        GoalieController.Instance.ResetState();
        StrikerController.Instance.ResetState();
    }
}