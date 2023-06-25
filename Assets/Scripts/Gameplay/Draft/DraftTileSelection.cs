using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DraftTileSelection : MonoBehaviour {

    [SerializeField] private DraftTokenSelection draftTokenSelection;
    [SerializeField] private EnemyDraftSelection enemyDraftSelection;

    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool sameTurn;
    [HideInInspector] public bool isPlayerTurn = true;
    [HideInInspector] public bool turnEndedEarly;
    [SerializeField] private float tileScaleSpeed;
    [SerializeField] private List<GameObject> tiles;

    [HideInInspector] public GameObject selectedTile;

    private void Start() 
    {
        // Set up scaling components of tokens
        SelectionManager.instance.timeUp += SwitchStates;
    }

    public void SwitchStates() 
    {
        if (isSelecting) { DeactivateTileSelection(); }
        else if (sameTurn && isPlayerTurn) { ActivateTileSelection(); }
    }

    private async void ActivateTileSelection() 
    {
        if (!draftTokenSelection.turnEndedEarly) { await Task.Delay(100); }

        // Add listener to spacePressed event
        SelectionManager.instance.tileClicked += OnTileClicked;

        // Highlight all selectable tiles
        foreach (GameObject tile in tiles) {
            tile.GetComponent<ToggleIndicators>().ToggleHighlight(true);
        }

        // Set up
        isSelecting = true;

        // Set up enemy turn
        SetEnemyTurnDelayed(0.1f);
        draftTokenSelection.isPlayerTurn = false;
        isPlayerTurn = false;
        sameTurn = false;
    }

    private void DeactivateTileSelection() 
    {
        if (selectedTile == null) { selectedTile = tiles[Random.Range(0, tiles.Count)]; }
        // Remove listener to spacePressed event
        SelectionManager.instance.tileClicked -= OnTileClicked;

        // Unhighlight and untarget all selectable tiles
        foreach (GameObject tile in tiles) {
            ToggleIndicators tileIndicator = tile.GetComponent<ToggleIndicators>();
            ScaleObject tileScaler = tile.GetComponent<ScaleObject>();
            tileIndicator.ToggleHighlight(false);
            tileIndicator.ToggleTarget(false);
            tileScaler.ScaleDown(tileScaleSpeed);
        }

        GameObject selectedTokenObject = draftTokenSelection.selectedToken;
        TokenMoveController tokenMoveController = selectedTokenObject.GetComponent<TokenMoveController>();
        tokenMoveController.StartMoveToPosition(selectedTile.transform.position);
        TokenMoveOptions tokenMoveOptions = selectedTokenObject.GetComponent<TokenMoveOptions>();
        tokenMoveOptions.currentTile = selectedTile;

        // Remove selected token from list
        tiles.Remove(selectedTile);
        selectedTile = null;
        ResetSelectedTokenDelayed(0.1f);
        if (!turnEndedEarly && isSelecting)
        {
            ColliderManager.instance.SwitchToTilesDeactivated();
            ColliderManager.instance.SwitchToTokensActivated();
            ResetSelection();
        }
        isSelecting = false;
    }

    private async void SetEnemyTurnDelayed(float delaySeconds) 
    {
        await Task.Delay((int)(delaySeconds * 1000));
        enemyDraftSelection.isEnemyTurn = true;
    }
    private async void ResetSelectedTokenDelayed(float delaySeconds)
    {
        await Task.Delay((int)(delaySeconds * 1000));
        draftTokenSelection.selectedToken = null;
        draftTokenSelection.turnEndedEarly = false;
    }
    private void ResetSelection() 
    {
        SelectionManager selectionManager = SelectionManager.instance;
        selectionManager.selectionMode = SelectionManager.SelectionMode.Token;
        selectionManager.ClearSelectedToken();
    }

    private void OnTileClicked(GameObject tile) 
    {
        if (!tiles.Contains(tile)) { return; }
        ColliderManager.instance.SwitchToTilesDeactivated();
        ColliderManager.instance.SwitchToTokensActivated();
        turnEndedEarly = true;
        selectedTile = tile;
        DeactivateTileSelection();
        ResetSelection();
        SelectionManager.instance.ForceTimeUp();
    }
}
