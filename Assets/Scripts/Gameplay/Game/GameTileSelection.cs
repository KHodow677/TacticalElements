using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameTileSelection : MonoBehaviour
{
    [SerializeField] private GameTokenSelection gameTokenSelection;
    [SerializeField] private EnemyGameSelection enemyGameSelection;

    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool sameTurn = false;
    [HideInInspector] public bool isPlayerTurn = true;
    [SerializeField] private float tileScaleSpeed;
    [SerializeField] private List<GameObject> allTiles;
    private List<GameObject> tiles;

    [HideInInspector] public GameObject selectedTile;

    private void Start()
    {
        SelectionManager.instance.gameModeChanged += OnGameplayMode;
        isSelecting = false;
        sameTurn = false;
    }

    private void OnGameplayMode()
    {
        SelectionManager.instance.timeUp += SwitchStates;
    }

    public void SwitchStates()
    {
        if (isSelecting) { DeactivateTileSelection(); }
        else if (sameTurn && isPlayerTurn) { ActivateTileSelection(); }
    }

    private void ActivateTileSelection()
    {
        SelectionManager.instance.tileClicked += OnTileClicked;
        FindMoveOptionsFromSelectedToken();
    }

    private void DeactivateTileSelection()
    {
        SelectionManager.instance.tileClicked -= OnTileClicked;

        foreach (GameObject tile in tiles)
        {
            ToggleIndicators tileIndicator = tile.GetComponent<ToggleIndicators>();
            ScaleObject tileScaler = tile.GetComponent<ScaleObject>();
            tileIndicator.ToggleHighlight(false);
            tileIndicator.ToggleTarget(false);
            tileScaler.ScaleDown(tileScaleSpeed);
        }

        GameObject selectedTokenObject = gameTokenSelection.selectedToken;
        TokenMoveController tokenMoveController = selectedTokenObject.GetComponent<TokenMoveController>();

        GameObject tokenAtTile = GetTokenAtPosition(selectedTile.transform.position, "Enemy");
        if (tokenAtTile != null) { tokenMoveController.StartMoveToPosition(selectedTile.transform.position, tokenAtTile); }
        else { tokenMoveController.StartMoveToPosition(selectedTile.transform.position); }

        isSelecting = false;
    }

    private async void FindMoveOptionsFromSelectedToken()
    {
        tiles = new List<GameObject>();
        while (gameTokenSelection.selectedToken == null)
        {
            await Task.Yield();
        }

        GameObject selectedToken = gameTokenSelection.selectedToken;
        ColliderManager.instance.SwitchToTokensActivated();
        tiles = GetAvailableTiles(selectedToken, "Player");

        foreach (GameObject tile in tiles)
        {
            ToggleIndicators tileIndicator = tile.GetComponent<ToggleIndicators>();
            tileIndicator.ToggleHighlight(true);
            
        }
        ColliderManager.instance.SwitchToTokensDeactivated();

        isSelecting = true;

        SetEnemyTurnDelayed(0.1f);
        gameTokenSelection.isPlayerTurn = false;
        isPlayerTurn = false;
        sameTurn = false;
    }

    private async void SetEnemyTurnDelayed(float delayInSeconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
        enemyGameSelection.isEnemyTurn = true;
    }

    public GameObject GetTileAtPosition(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f, LayerMask.GetMask("Tile"));

        if (colliders != null && colliders.Length > 0)
        {
            return colliders[0].gameObject;
        }
        return null;
    }

    public GameObject GetTokenAtPosition(Vector3 position, string side)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f, LayerMask.GetMask("Token"));

        if (colliders != null && colliders.Length > 0)
        {
            if (colliders[0].gameObject.tag == side) { return colliders[0].gameObject; }
        }
        return null;
    }

    public List<GameObject> GetAvailableTiles(GameObject token, string side)
    {
        List<GameObject> availableTiles = new List<GameObject>();
        TokenMoveOptions moveOptions = token.GetComponent<TokenMoveOptions>();
        foreach (Vector3 moveOffset in moveOptions.moveOffsetOptions)
        {
            Vector3 tilePosition = token.transform.position + moveOffset;
            GameObject tokenObject = GetTokenAtPosition(tilePosition, side);
            if (tokenObject != null) { continue; }
            GameObject tileObject = GetTileAtPosition(tilePosition);
            if (tileObject == null) { continue; }
            availableTiles.Add(tileObject);
        }
        return availableTiles;
    }

    public void HighlightAvailableTiles(GameObject token, string side)
    {
        List<GameObject> availableTiles = GetAvailableTiles(token, side);
        foreach (GameObject tile in availableTiles)
        {
            tile.GetComponent<ToggleIndicators>().ToggleHighlight(true);
        }
    }

    public void UnhighlightAvailableTiles(GameObject token, string side)
    {
        List<GameObject> availableTiles = GetAvailableTiles(token, side);
        foreach (GameObject tile in availableTiles)
        {
            tile.GetComponent<ToggleIndicators>().ToggleHighlight(false);
        }
    }

    private void OnTileClicked(GameObject tile, bool physicallyClicked)
    {
        if (!tiles.Contains(tile)) { return; }
        ColliderManager.instance.SwitchToTilesDeactivated();
        ColliderManager.instance.SwitchToTokensActivated();
        selectedTile = tile;
        DeactivateTileSelection();
        SelectionManager selectionManager = SelectionManager.instance;
        selectionManager.selectionMode = SelectionManager.SelectionMode.Token;
        selectionManager.ClearSelectedToken();
        selectionManager.ForceTimeUp();
    }
}
