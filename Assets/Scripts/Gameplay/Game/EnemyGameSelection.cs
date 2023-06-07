using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyGameSelection : MonoBehaviour {
    private static EnemyGameSelection _instance;
    public static EnemyGameSelection instance { get { return _instance; } }
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
        if (isEnemyTurn) { StartCoroutine(SelectAndMoveToken()); }
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
            if (childObject.activeInHierarchy == false) { continue; }

            int tileCount = GameTileSelection.instance.GetAvailableTiles(childObject, "Enemy").Count;
            if (tileCount == 0) { continue; }

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
            StartCoroutine(sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, "Win Scene"));
            yield break;
        }

        // Set up scaling and state components of tokens
        tokenScalers = new List<ScaleObject>();
        for (int i = 0; i < tokens.Count; i++) {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
        }

        selectedTokenScaler = tokenScalers[UnityEngine.Random.Range(0, tokenScalers.Count)];
        bool shouldBreak = false;

        foreach (GameObject token in tokens)
        {
            List<GameObject> availableSpots = GameTileSelection.instance.GetAvailableTiles(token, "Enemy");
            foreach (GameObject spot in availableSpots) {
                GameObject tokenAtSpot = GameTileSelection.instance.GetTokenAtPosition(spot.transform.position, "Player");
                if (tokenAtSpot != null) {
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

        // Set up Player turn
        GameTokenSelection.instance.isPlayerTurn = true;
        GameTileSelection.instance.isPlayerTurn = true;
        GameTileSelection.instance.sameTurn = false;
        isEnemyTurn = false;

        yield return new WaitForSeconds(moveController.moveDuration);
        SelectionManager.instance.UnpauseClock();

        selectedTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
        // Set selected tile
        for (int i = 0; i < tiles.Count; i++) {
            GameObject tokenObject = GameTileSelection.instance.GetTokenAtPosition(tiles[i].transform.position, "Player");
            if (tokenObject != null) {
                selectedTile = tiles[i];
                break;
            }
        }
        TokenMoveController tokenMoveController = selectedToken.GetComponent<TokenMoveController>();

        GameObject tokenAtTile = GameTileSelection.instance.GetTokenAtPosition(selectedTile.transform.position, "Player");
        if (tokenAtTile != null) { tokenMoveController.StartMoveToPosition(selectedTile.transform.position, tokenAtTile, resetClockAfterMove: true); }
        else { tokenMoveController.StartMoveToPosition(selectedTile.transform.position, resetClockAfterMove: true); }

        // Tear down
        isSelecting = false;
    }
}
