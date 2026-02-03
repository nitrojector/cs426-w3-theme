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
    
    private readonly List<VisualElement> _blueScoreList = new();
    private readonly List<VisualElement> _yellowScoreList = new();

    private VisualElement _winMessage;

    private InputAction _anyKeyAction;
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
        _anyKeyAction = InputSystem.actions.FindAction("Any");
        _toggleHalfAction.started += (ctx) => { GameState.FirstHalf = !GameState.FirstHalf; };
        _uiDocument = GetComponent<UIDocument>();
    }

    void Start()
    {
        _rootEl = GetComponent<UIDocument>().rootVisualElement;
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

        if (_anyKeyAction.triggered && !RoundStarted)
        {
            RoundStarted = true;
        }
            
        if (!RoundStarted)
            return;
        
        _roundTime += Time.deltaTime;

        _blueScoreLabel.text = GameState.ScoreOf(Team.Blue).ToString();
        _yellowScoreLabel.text = GameState.ScoreOf(Team.Yellow).ToString();
        
        // TODO: Update round time left on screen wit UI

        if (RoundTimeLeft <= 0.0f)
        {
            GameState.SetRoundWinner(GameState.FirstHalf ? GameState.OtherTeam(GameState.FirstHalfStriker) : GameState.FirstHalfStriker);
        
            StartCoroutine(DelayedNextRound());
        }
            

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
        GameState.SetRoundWinner(Team.Blue);
        StartCoroutine(DelayedNextRound());
    }

    public void ScoreYellow()
    {
        GameState.SetRoundWinner(Team.Yellow);
        StartCoroutine(DelayedNextRound());
    }
    
    public IEnumerator DelayedNextRound(float delay = 2.0f)
    {
        yield return new WaitForSeconds(delay);
        NextRound();
    }
    
    public void NextRound()
    {
        GameState.AdvanceRound();
        RoundStarted = false;
        _roundTime = 0.0f;
    }

    void Reset()
    {
        RoundStarted = false;
        _roundTime = 0.0f;
        GoalieController.Instance.ResetState();
        StrikerController.Instance.ResetState();
        GameState.Reset();
    }
}