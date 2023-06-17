using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DraftTileSelection : MonoBehaviour {

    [SerializeField] private DraftTokenSelection draftTokenSelection;
    [SerializeField] private EnemyDraftSelection enemyDraftSelection;

    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool sameTurn;
    [HideInInspector] public bool isPlayerTurn = true;
    [SerializeField] private float tileScaleSpeed;
    [SerializeField] private List<GameObject> tiles;
    private List<ScaleObject> tileScalers;
    private List<ToggleIndicators> tileIndicators;
    private ScaleObject selectedTileScaler;
    private ToggleIndicators selectedTileIndicator;
    [HideInInspector] public GameObject selectedTile;

    private void Start() {
        // Set up scaling components of tokens
        tileScalers = new List<ScaleObject>();
        tileIndicators = new List<ToggleIndicators>();
        for (int i = 0; i < tiles.Count; i++) {
            tileScalers.Add(tiles[i].GetComponent<ScaleObject>());
            tileIndicators.Add(tiles[i].GetComponent<ToggleIndicators>());
        }
        SelectionManager.instance.timeUp += SwitchStates;
    }

    /// <summary>
    /// Switches between selecting and not selecting tiles.
    /// </summary>
    public void SwitchStates() {
        if (isSelecting) { DeactivateTileSelection(tileScaleSpeed); }
        else if (sameTurn && isPlayerTurn) { ActivateTileSelection(tileScaleSpeed); }
    }

    /// <summary>
    /// Activates tile selection.
    /// </summary>
    /// <param name="initialTileScaleSpeed"></param>
    private void ActivateTileSelection(float initialTileScaleSpeed) {
        // Add listener to spacePressed event
        SelectionManager.instance.spacePressed += OnSpacePressed;

        // Highlight all selectable tiles
        foreach (ToggleIndicators tileIndicator in tileIndicators) {
            tileIndicator.ToggleHighlight(true);
        }

        // Set up
        selectedTileScaler = tileScalers[0];
        selectedTileIndicator = tileIndicators[0];
        selectedTileScaler.ScaleUp(initialTileScaleSpeed);
        selectedTileIndicator.ToggleTarget(true);
        isSelecting = true;

        // Set up enemy turn
        SetEnemyTurnDelayed();
        draftTokenSelection.isPlayerTurn = false;
        isPlayerTurn = false;
        sameTurn = false;
    }

    /// <summary>
    /// Deactivates tile selection.
    /// </summary>
    /// <param name="initialTileScaleSpeed"></param>
    private void DeactivateTileSelection(float initialTileScaleSpeed) {
        // Remove listener to spacePressed event
        SelectionManager.instance.spacePressed -= OnSpacePressed;

        draftTokenSelection.HandleDisplayToken(false);

        // Unhighlight and untarget all selectable tiles
        foreach (ToggleIndicators tileIndicator in tileIndicators) {
            tileIndicator.ToggleHighlight(false);
            tileIndicator.ToggleTarget(false);
        }

        // Set selected tile
        selectedTile = selectedTileScaler.gameObject;
        GameObject selectedTokenObject = draftTokenSelection.selectedToken;
        TokenMoveController tokenMoveController = selectedTokenObject.GetComponent<TokenMoveController>();
        tokenMoveController.StartMoveToPosition(selectedTile.transform.position);
        TokenMoveOptions tokenMoveOptions = selectedTokenObject.GetComponent<TokenMoveOptions>();
        tokenMoveOptions.currentTile = selectedTile;

        // Remove selected token from lists
        tiles.Remove(selectedTile);
        tileScalers.Remove(selectedTileScaler);
        tileIndicators.Remove(selectedTile.GetComponent<ToggleIndicators>());

        // Tear down
        selectedTileScaler.ScaleDown(initialTileScaleSpeed);
        isSelecting = false;
    }

    private async void SetEnemyTurnDelayed() {
        await Task.Delay((int)(0.5f * SelectionManager.instance.timePerTurn * 1000));
        enemyDraftSelection.isEnemyTurn = true;
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
