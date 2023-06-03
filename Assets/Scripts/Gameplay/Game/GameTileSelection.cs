using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTileSelection : MonoBehaviour
{
    private static GameTileSelection _instance;
    public static GameTileSelection instance { get { return _instance; } }
    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool sameTurn = false;
    [HideInInspector] public bool isPlayerTurn = true;
    [SerializeField] private float tileScaleSpeed;
    [SerializeField] List<GameObject> allTiles;
    [HideInInspector] private List<GameObject> tiles;
    private List<ScaleObject> tileScalers;
    private List<ToggleIndicators> tileIndicators;
    private ScaleObject selectedTileScaler;
    private ToggleIndicators selectedTileIndicator;
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
        sameTurn = false;
    }

    private void OnGameplayMode() {
        // Subscribe to switch states
        SelectionManager.instance.timeUp += SwitchStates;
    }

    /// <summary>
    /// Switches between selecting and not selecting tiles.
    /// </summary>
    public void SwitchStates() {
        if (isSelecting) { DeactivateTileSelection(); }
        else if (sameTurn && isPlayerTurn) { ActivateTileSelection(); }
    }

    /// <summary>
    /// Activates tile selection.
    /// </summary>
    /// <param name="initialTileScaleSpeed"></param>
    private void ActivateTileSelection() {
        // Add listener to spacePressed event
        SelectionManager.instance.spacePressed += OnSpacePressed;

        StartCoroutine(FindMoveOptionsFromSelectedToken());
    }

    /// <summary>
    /// Deactivates tile selection.
    /// </summary>
    /// <param name="initialTileScaleSpeed"></param>
    private void DeactivateTileSelection() {
        // Remove listener to spacePressed event
        SelectionManager.instance.spacePressed -= OnSpacePressed;

        // Unhighlight and untarget all selectable tiles
        foreach (ToggleIndicators tileIndicator in tileIndicators) {
            tileIndicator.ToggleHighlight(false);
            tileIndicator.ToggleTarget(false);
        }

        // Set selected tile
        selectedTile = selectedTileScaler.gameObject;
        GameObject selectedTokenObject = GameTokenSelection.instance.selectedToken;
        TokenMoveController tokenMoveController = selectedTokenObject.GetComponent<TokenMoveController>();
        tokenMoveController.StartMoveToPosition(selectedTile.transform.position);

        // Remove selected token from lists
        tiles.Remove(selectedTile);
        tileScalers.Remove(selectedTileScaler);
        tileIndicators.Remove(selectedTile.GetComponent<ToggleIndicators>());

        // Tear down
        selectedTileScaler.ScaleDown(tileScaleSpeed);
        isSelecting = false;
    }

    private IEnumerator FindMoveOptionsFromSelectedToken() {
        while (GameTokenSelection.instance.selectedToken == null) {
            yield return null;
        }
        GameObject selectedToken = GameTokenSelection.instance.selectedToken;
        TokenMoveOptions selectedTokenMoveOptions = selectedToken.GetComponent<TokenMoveOptions>();
        foreach (Vector3 moveOffset in selectedTokenMoveOptions.moveOffsetOptions) {
            Vector3 tilePosition = selectedToken.transform.position + moveOffset;
            if (GetTileAtPosition(tilePosition) == null) { continue; }
            tiles.Add(GetTileAtPosition(tilePosition));
        }

        // Set up scaling components of tokens
        tileScalers = new List<ScaleObject>();
        tileIndicators = new List<ToggleIndicators>();
        for (int i = 0; i < tiles.Count; i++) {
            tileScalers.Add(tiles[i].GetComponent<ScaleObject>());
            tileIndicators.Add(tiles[i].GetComponent<ToggleIndicators>());
        }

        // Highlight all selectable tiles
        foreach (ToggleIndicators tileIndicator in tileIndicators) {
            tileIndicator.ToggleHighlight(true);
        }

        // Set up
        selectedTileScaler = tileScalers[0];
        selectedTileIndicator = tileIndicators[0];
        selectedTileScaler.ScaleUp(tileScaleSpeed);
        selectedTileIndicator.ToggleTarget(true);
        isSelecting = true;

        // Set up enemy turn
        StartCoroutine(SetEnemyTurnDelayed());
        GameTokenSelection.instance.isPlayerTurn = false;
        isPlayerTurn = false;
        sameTurn = false;
    }

    private IEnumerator SetEnemyTurnDelayed() {
        yield return new WaitForSeconds(0.5f * SelectionManager.instance.timePerTurn);
        EnemyDraftSelection.instance.isEnemyTurn = true;
    }

    private GameObject GetTileAtPosition(Vector3 position) {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f, LayerMask.GetMask("Tile"));

        if (colliders != null && colliders.Length > 0) {
            return colliders[0].gameObject;
        }
        return null;
    }

    /// <summary>
    /// Called when space is pressed.
    /// </summary>
    private void OnSpacePressed() {
        // Scale down current tile
        selectedTileScaler.ScaleDown(tileScaleSpeed);
        selectedTileIndicator.ToggleTarget(false);

        // Get next tile, bounce back if end of list is reached
        int currentIndex = tileScalers.IndexOf(selectedTileScaler);
        int nextIndex = (currentIndex + 1) % tileScalers.Count;
        selectedTileScaler = tileScalers[nextIndex];
        selectedTileIndicator = tileIndicators[nextIndex];
        // Scale up next tile
        selectedTileScaler.ScaleUp(tileScaleSpeed);
        selectedTileIndicator.ToggleTarget(true);
    }

}
