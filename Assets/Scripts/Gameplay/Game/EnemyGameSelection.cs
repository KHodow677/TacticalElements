using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class EnemyGameSelection : MonoBehaviour {

    [SerializeField] private GameTokenSelection gameTokenSelection;
    [SerializeField] private GameTileSelection gameTileSelection;

    [SerializeField] private SceneFader sceneFader;
    [SerializeField] private float timePerTurn;
    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isEnemyTurn = false;
    [SerializeField] private float tokenScaleSpeed;
    [SerializeField] public List<GameObject> tokens;
    [SerializeField] public List<GameObject> tiles;
    [HideInInspector] public List<ScaleObject> tokenScalers;
    private ScaleObject selectedTokenScaler;
    [HideInInspector] public GameObject selectedToken;
    [HideInInspector] public GameObject selectedTile;

    private void Start() {
        SelectionManager.instance.gameModeChanged += OnGameplayMode;
        isSelecting = false;
    }

    private void OnGameplayMode() {
        // Subscribe to switch states
        SelectionManager.instance.timeUp += SwitchStates;
    }

    /// <summary>
    /// Switches between selecting and not selecting tiles.
    /// </summary>
    public void SwitchStates() {
        if (isEnemyTurn) { SelectAndMoveToken(); }
    }

    public async void SelectAndMoveToken() {
        SelectionManager.instance.PauseClock();
        await Task.Delay(TimeSpan.FromSeconds(timePerTurn));

        // Load up list of player tokens from child objects
        tokens = new List<GameObject>();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            GameObject childObject = childTransform.gameObject;
            if (!childObject.activeInHierarchy) { continue; }

            int tileCount = gameTileSelection.GetAvailableTiles(childObject, "Enemy").Count;
            if (tileCount == 0) { continue; }

            tokens.Add(childObject);
        }

        // Set up scaling and state components of tokens
        tokenScalers = new List<ScaleObject>();
        for (int i = 0; i < tokens.Count; i++)
        {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
        }

        if (tokens.Count == 0)
        {
            Debug.Log("Tokens empty");
            SelectionManager.instance.PauseClock();
            SelectionManager.instance.gameMode = SelectionManager.GameMode.GameOver;
            StartCoroutine(sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, "Win Scene"));
            return;
        }

        // Set up scaling and state components of tokens
        tokenScalers = new List<ScaleObject>();
        for (int i = 0; i < tokens.Count; i++)
        {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
        }

        selectedTokenScaler = tokenScalers[UnityEngine.Random.Range(0, tokenScalers.Count)];
        bool shouldBreak = false;

        foreach (GameObject token in tokens)
        {
            List<GameObject> availableSpots = gameTileSelection.GetAvailableTiles(token, "Enemy");
            foreach (GameObject spot in availableSpots)
            {
                GameObject tokenAtSpot = gameTileSelection.GetTokenAtPosition(spot.transform.position, "Player");
                if (tokenAtSpot != null)
                {
                    selectedTokenScaler = token.GetComponent<ScaleObject>();
                    shouldBreak = true;
                    break;
                }
            }
            if (shouldBreak) { break; }
        }

        // Set up
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        isSelecting = true;

        // Set selected token
        selectedToken = selectedTokenScaler.gameObject;
        TokenMoveController moveController = selectedToken.GetComponent<TokenMoveController>();

        tiles = new List<GameObject>();
        while (gameTokenSelection.selectedToken == null)
        {
            await Task.Yield();
        }
        TokenMoveOptions selectedTokenMoveOptions = selectedToken.GetComponent<TokenMoveOptions>();
        foreach (Vector3 moveOffset in selectedTokenMoveOptions.moveOffsetOptions)
        {
            Vector3 tilePosition = selectedToken.transform.position + moveOffset;
            GameObject tokenObject = gameTileSelection.GetTokenAtPosition(tilePosition, "Enemy");
            if (tokenObject != null) { continue; }
            GameObject tileObject = gameTileSelection.GetTileAtPosition(tilePosition);
            if (tileObject == null) { continue; }
            tiles.Add(tileObject);
        }

        // Set up Player turn
        gameTokenSelection.isPlayerTurn = true;
        gameTileSelection.isPlayerTurn = true;
        gameTileSelection.sameTurn = false;
        isEnemyTurn = false;

        await Task.Delay(TimeSpan.FromSeconds(moveController.moveDuration));
        SelectionManager.instance.UnpauseClock();

        selectedTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
        // Set selected tile
        for (int i = 0; i < tiles.Count; i++)
        {
            GameObject tokenObject = gameTileSelection.GetTokenAtPosition(tiles[i].transform.position, "Player");
            if (tokenObject != null)
            {
                selectedTile = tiles[i];
                break;
            }
        }
        TokenMoveController tokenMoveController = selectedToken.GetComponent<TokenMoveController>();

        GameObject tokenAtTile = gameTileSelection.GetTokenAtPosition(selectedTile.transform.position, "Player");
        if (tokenAtTile != null) { tokenMoveController.StartMoveToPosition(selectedTile.transform.position, tokenAtTile, resetClockAfterMove: true); }
        else { tokenMoveController.StartMoveToPosition(selectedTile.transform.position, resetClockAfterMove: true); }

        // Tear down
        isSelecting = false;
    }
}
