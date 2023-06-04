using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGameSelection : MonoBehaviour {
    private static EnemyGameSelection _instance;
    public static EnemyGameSelection instance { get { return _instance; } }
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

    private void Awake() {
        // Ensure only one instance of the class exists
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
            return;
        }
        else { _instance = this; }
    }

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
        if (isSelecting) { SelectionManager.instance.ResetClock(); }
        else if (isEnemyTurn) { StartCoroutine(SelectAndMoveToken()); }
    }

    public IEnumerator SelectAndMoveToken() {
        SelectionManager.instance.PauseClock();
        yield return new WaitForSeconds(timePerTurn);

        // Load up list of player tokens from child objects
        tokens = new List<GameObject>();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++) {
            Transform childTransform = transform.GetChild(i);
            GameObject childObject = childTransform.gameObject;

            if (GameTileSelection.instance.GetAvailableTiles(childObject).Count == 0) { continue; }
            
            tokens.Add(childObject);
        }

        // Set up scaling and state components of tokens
        tokenScalers = new List<ScaleObject>();
        for (int i = 0; i < tokens.Count; i++) {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
        }

        if(tokens.Count == 0) {
            Debug.Log("Tokens empty");
            SelectionManager.instance.PauseClock();
            SelectionManager.instance.gameMode = SelectionManager.GameMode.GameOver;
            yield break;
        }

        // Set up scaling and state components of tokens
        tokenScalers = new List<ScaleObject>();
        for (int i = 0; i < tokens.Count; i++) {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
        }

        // Set up
        selectedTokenScaler = tokenScalers[UnityEngine.Random.Range(0, tokenScalers.Count)];
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        isSelecting = true;

        // Set selected token
        selectedToken = selectedTokenScaler.gameObject;
        TokenMoveController moveController = selectedToken.GetComponent<TokenMoveController>();


        tiles = new List<GameObject>();
        while (GameTokenSelection.instance.selectedToken == null) {
            yield return null;
        }
        TokenMoveOptions selectedTokenMoveOptions = selectedToken.GetComponent<TokenMoveOptions>();
        foreach (Vector3 moveOffset in selectedTokenMoveOptions.moveOffsetOptions) {
            Vector3 tilePosition = selectedToken.transform.position + moveOffset;
            GameObject tokenObject = GameTileSelection.instance.GetTokenAtPosition(tilePosition, "Enemy");
            if (tokenObject != null) { continue; }
            GameObject tileObject = GameTileSelection.instance.GetTileAtPosition(tilePosition);
            if (tileObject == null) { continue; }
            tiles.Add(tileObject);
        }

        // Set up enemy turn
        StartCoroutine(SetPlayerTurnDelayed());
        GameTokenSelection.instance.isPlayerTurn = false;
        isEnemyTurn = false;
        GameTileSelection.instance.sameTurn = false;

        yield return new WaitForSeconds(moveController.moveDuration);
        SelectionManager.instance.UnpauseClock();

        // Set selected tile
        selectedTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
        TokenMoveController tokenMoveController = selectedToken.GetComponent<TokenMoveController>();
        tokenMoveController.StartMoveToPosition(selectedTile.transform.position);

        // Tear down
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        isSelecting = false;
        isEnemyTurn = false;
        SelectionManager.instance.ResetClock();
    }

    private IEnumerator SetPlayerTurnDelayed() {
        yield return new WaitForSeconds(0.5f * SelectionManager.instance.timePerTurn);
        GameTokenSelection.instance.isPlayerTurn = true;
        GameTileSelection.instance.isPlayerTurn = true;
        GameTileSelection.instance.sameTurn = false;
    }
}
