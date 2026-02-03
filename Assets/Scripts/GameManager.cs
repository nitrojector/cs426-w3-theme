using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool RoundStarted
    {
        get => GameState.RoundInProgress;
        set => GameState.RoundInProgress = value;
    }

    public const float TotalRoundTime = 10.0f;
    public float RoundTimeLeft => Mathf.Max(0.0f, TotalRoundTime - _roundTime);
    private float _roundTime = 0.0f;

    private UIDocument _uiDocument;
    private VisualElement _rootEl;

    private Label _blueScoreLabel;
    private Label _yellowScoreLabel;
    private Label _winLabel;

    private readonly List<VisualElement> _blueScoreList = new();
    private readonly List<VisualElement> _yellowScoreList = new();

    private VisualElement _winMessage;

    private InputAction _anyKeyAction;
    private InputAction _resetAction;
    private InputAction _quitAction;
    private InputAction _toggleHalfAction;
    private bool _roundEnding = false;
    private bool _gameOver = false;
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
        _anyKeyAction = InputSystem.actions.FindAction("Any");
        _toggleHalfAction.started += (ctx) =>
        {
            if (!_gameOver) GameState.FirstHalf = !GameState.FirstHalf;
        };
        _uiDocument = GetComponent<UIDocument>();
    }

    void Start()
    {
        _rootEl = GetComponent<UIDocument>().rootVisualElement;
        _winLabel = _rootEl.Q<Label>("win-text");
        _winLabel.style.display = DisplayStyle.None;
        _blueScoreLabel = _rootEl.Q<Label>("blue-score");
        _yellowScoreLabel = _rootEl.Q<Label>("yellow-score");
        for (int i = 0; i < 5; i++)
        {
            var yel = _rootEl.Q<VisualElement>($"y-{i}");
            var bel = _rootEl.Q<VisualElement>($"b-{i}");
            if (yel == null || bel == null)
            {
                Debug.LogError($"Could not find score element for index {i}");
            }

            _yellowScoreList.Add(yel);
            _blueScoreList.Add(bel);
        }
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

        if (_anyKeyAction.triggered && !RoundStarted && !_gameOver)
        {
            RoundStarted = true;
        }

        UpdateScoreUI();

        if (!RoundStarted)
            return;

        _roundTime += Time.deltaTime;

        // TODO: Update round time left on screen wit UI

        if (RoundTimeLeft <= 0.0f)
        {
            GameState.SetRoundWinner(GameState.FirstHalf
                ? GameState.OtherTeam(GameState.FirstHalfStriker)
                : GameState.FirstHalfStriker);

            if (!_roundEnding)
            {
                _roundEnding = true;
                StartCoroutine(DelayedNextRound());
            }
        }
    }

    private void UpdateScoreUI()
    {
        _blueScoreLabel.text = GameState.ScoreOf(Team.Blue).ToString();
        _yellowScoreLabel.text = GameState.ScoreOf(Team.Yellow).ToString();

        for (int i = 0; i < 5; i++)
        {
            _yellowScoreList[i].ClearClassList();
            _blueScoreList[i].ClearClassList();
            switch (GameState.TeamRecords[Team.Blue][i])
            {
                case 1:
                    _blueScoreList[i].AddToClassList("circle-green");
                    break;
                case -1:
                    _blueScoreList[i].AddToClassList("circle-red");
                    break;
                case 0:
                    _blueScoreList[i].AddToClassList("circle-empty");
                    break;
            }

            switch (GameState.TeamRecords[Team.Yellow][i])
            {
                case 1:
                    _yellowScoreList[i].AddToClassList("circle-green");
                    break;
                case -1:
                    _yellowScoreList[i].AddToClassList("circle-red");
                    break;
                case 0:
                    _yellowScoreList[i].AddToClassList("circle-empty");
                    break;
            }
        }
    }

    public void ScoreBlue()
    {
        if (_gameOver) return;
        GameState.SetRoundWinner(Team.Blue);
        if (!_roundEnding)
        {
            _roundEnding = true;
            StartCoroutine(DelayedNextRound());
        }
    }

    public void ScoreYellow()
    {
        if (_gameOver) return;
        GameState.SetRoundWinner(Team.Yellow);
        if (!_roundEnding)
        {
            _roundEnding = true;
            StartCoroutine(DelayedNextRound());
        }
    }

    public IEnumerator DelayedNextRound(float delay = 2.0f)
    {
        yield return new WaitForSeconds(delay);
        NextRound();
    }

    public void NextRound()
    {
        Debug.Log($"NextRound() before Advance: roundNo={GameState.roundNo}, FirstHalf={GameState.FirstHalf}");
        GameState.AdvanceRound();
        Debug.Log($"NextRound() after Advance: roundNo={GameState.roundNo}, FirstHalf={GameState.FirstHalf}");

        if (GameState.IsGameOver())
        {
            // Stop any further input and show winner
            RoundStarted = false;
            ShowWinner();
            return;
        }

        ResetSoft();
    }

    void ShowWinner()
    {
        _winLabel.style.display = DisplayStyle.Flex;
        var winner = GameState.GetWinner();
        string msg = winner switch
        {
            Team.Blue => "Blue Wins!",
            Team.Yellow => "Yellow Wins!",
            _ => "Draw"
        };

        if (_winLabel != null)
            _winLabel.text = msg;

        // Stop further input
        RoundStarted = false;
        _gameOver = true;
    }

    void ResetSoft()
    {
        RoundStarted = false;
        _roundTime = 0.0f;
        GameState.RoundEnd = false;
        _roundEnding = false;
        _gameOver = false;
        GoalieController.Instance.ResetState();
        StrikerController.Instance.ResetState();
    }

    void Reset()
    {
        _winLabel.style.display = DisplayStyle.None;
        ResetSoft();
        GameState.Reset();
    }
}