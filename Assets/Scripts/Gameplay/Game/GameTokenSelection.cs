using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

public class GameTokenSelection : MonoBehaviour
{
    [SerializeField] private GameTileSelection gameTileSelection;
    [SerializeField] private EnemyGameSelection enemyGameSelection;
    [SerializeField] private GameObject tokenDisplayObject;
    [SerializeField] private SceneFader sceneFader;
    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isPlayerTurn = true;
    [SerializeField] private float tokenScaleSpeed;
    [SerializeField] public List<GameObject> tokens;

    [HideInInspector] public GameObject selectedToken;

    private void Start()
    {
        SelectionManager.instance.gameModeChanged += OnGameplayMode;
        isSelecting = false;
    }

    private void OnGameplayMode()
    {
        GameplayManager.instance.SetUpEngine();
        SelectionManager.instance.timeUp += SwitchStates;
        SelectionManager.instance.ForceTimeUp();
    }

    public void SwitchStates()
    {
        if (isSelecting) { DeactivateTokenSelection(); }
        else if (isPlayerTurn) { ActivateTokenSelection(); }
    }

    private void ActivateTokenSelection()
    {
        SelectionManager.instance.tokenClicked += OnTokenClicked;
        SelectionManager.instance.tokenHovered += OnTokenHovered;
        SelectionManager.instance.tokenUnhovered += OnTokenUnhovered;

        tokens = GetActivePlayerTokens();
        if (tokens.Count == 0)
        {
            GameOver();
            return;
        }
        
        SetSameTurnDelayed(0.1f);
        isSelecting = true;
    }
    private List<GameObject> GetActivePlayerTokens()
    {
        List<GameObject> activeTokens = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            int moveCount = GameplayManager.instance.GetLegalMoves(transform.GetChild(i).name).Count;
            if (moveCount > 0)
            {
                activeTokens.Add(transform.GetChild(i).gameObject);
            }
        }
        Debug.Log(activeTokens.Count);
        return activeTokens;
    }

    private void GameOver()
    {
        SelectionManager.instance.tokenClicked -= OnTokenClicked;
        SelectionManager.instance.tokenHovered -= OnTokenHovered;
        SelectionManager.instance.tokenUnhovered -= OnTokenUnhovered;

        SelectionManager.instance.PauseClock();
        SelectionManager.instance.gameMode = SelectionManager.GameMode.GameOver;
        StartCoroutine(sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, "Lose Scene"));
    }

    private async void SetSameTurnDelayed(float delayInSeconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
        gameTileSelection.sameTurn = true;
        selectedToken = null;
    }

    private void DeactivateTokenSelection()
    {
        if (selectedToken == null)
        {
            selectedToken = tokens[UnityEngine.Random.Range(0, tokens.Count)];
            SelectionManager.instance.OnTokenSelect(selectedToken);
            SelectionManager.instance.OnTokenClicked(selectedToken, false);
            return;
        }

        SelectionManager.instance.tokenClicked -= OnTokenClicked;
        SelectionManager.instance.tokenHovered -= OnTokenHovered;
        SelectionManager.instance.tokenUnhovered -= OnTokenUnhovered;

        isSelecting = false;
    }

    private void OnTokenHovered(GameObject token)
    {
        if (!tokens.Contains(token)) { return; }
        gameTileSelection.HighlightAvailableTiles(token, "Player 1");
    }
    private void OnTokenUnhovered(GameObject token)
    {
        if (!tokens.Contains(token)) { return; }
        gameTileSelection.UnhighlightAvailableTiles(token, "Player 1");
    }

    private void ResetSelection()
    {
        SelectionManager selectionManager = SelectionManager.instance;
        selectionManager.selectionMode = SelectionManager.SelectionMode.Tile;
        selectionManager.playerTurn = SelectionManager.PlayerTurn.Player2;
        selectionManager.SetSelectedToken(selectedToken);
    }

    private void OnTokenClicked(GameObject token, bool physicallyClicked)
    {
        if (!tokens.Contains(token)) { return; }
        ColliderManager.instance.SwitchToTilesActivated();
        ColliderManager.instance.SwitchToTokensDeactivated();
        selectedToken = token;
        DeactivateTokenSelection();
        ResetSelection();

        if (physicallyClicked)
        {
            SelectionManager.instance.ForceTimeUp();
        }
    }
}
