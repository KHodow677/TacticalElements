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
        ColliderManager.instance.SwitchToTilesActivated();
        List<GameObject> activeTokens = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childObject = transform.GetChild(i).gameObject;
            if (!childObject.activeInHierarchy) { continue; }
            int tileCount = gameTileSelection.GetAvailableTiles(childObject, "Player").Count;
            if (tileCount == 0) { continue; }
            activeTokens.Add(childObject);
        }
        ColliderManager.instance.SwitchToTilesDeactivated();
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
        SceneManager.LoadScene("Lose Scene");
    }

    private async void SetSameTurnDelayed(float delayInSeconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
        gameTileSelection.sameTurn = true;
        selectedToken = null;
    }

    private void DeactivateTokenSelection()
    {
        SelectionManager.instance.tokenClicked -= OnTokenClicked;
        SelectionManager.instance.tokenHovered -= OnTokenHovered;
        SelectionManager.instance.tokenUnhovered -= OnTokenUnhovered;

        isSelecting = false;
    }

    private void OnTokenHovered(GameObject token)
    {
        ColliderManager.instance.SwitchToTilesActivated();
        if (!tokens.Contains(token)) { return; }
        gameTileSelection.HighlightAvailableTiles(token, "Player");
        ColliderManager.instance.SwitchToTilesDeactivated();
    }
    private void OnTokenUnhovered(GameObject token)
    {
        ColliderManager.instance.SwitchToTilesActivated();
        if (!tokens.Contains(token)) { return; }
        gameTileSelection.UnhighlightAvailableTiles(token, "Player");
        ColliderManager.instance.SwitchToTilesDeactivated();
    }

    private void OnTokenClicked(GameObject token, bool physicallyClicked)
    {
        if (!tokens.Contains(token)) { return; }
        ColliderManager.instance.SwitchToTilesActivated();
        ColliderManager.instance.SwitchToTokensDeactivated();
        selectedToken = token;
        DeactivateTokenSelection();
        SelectionManager selectionManager = SelectionManager.instance;
        selectionManager.selectionMode = SelectionManager.SelectionMode.Tile;
        selectionManager.playerTurn = SelectionManager.PlayerTurn.Player2;
        selectionManager.SetSelectedToken(selectedToken);
        selectionManager.ForceTimeUp();
    }
}
