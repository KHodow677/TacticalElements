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
        if (selectedTile == null)
        {
            selectedTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
            SelectionManager.instance.OnTileClicked(selectedTile, false);
            return;
        }

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
        GameObject tokenAtTile = GetTokenAtPosition(selectedTile.transform.position, "Player 2");

        GameplayManager.instance.MakeMove(selectedTokenObject.name, selectedTile.transform.position);

        isSelecting = false;
    }

    private async void FindMoveOptionsFromSelectedToken()
    {
        while (gameTokenSelection.selectedToken == null)
        {
            await Task.Yield();
        }

        GameObject selectedTokenObject = gameTokenSelection.selectedToken;
        List<Vector3> tilePosList = GameplayManager.instance.GetPossibleTilePos(selectedTokenObject.name);
        tiles = new List<GameObject>();
        for (int i = 0; i < tilePosList.Count; i++)
        {
            tiles.Add(GetTileAtPosition(tilePosList[i]));
        }

        foreach (GameObject tile in tiles)
        {
            ToggleIndicators tileIndicator = tile.GetComponent<ToggleIndicators>();
            tileIndicator.ToggleHighlight(true);
        }

        // Set up
        isSelecting = true;

        // Set up enemy turn
        SetEnemyTurnDelayed(0.1f);
        gameTokenSelection.isPlayerTurn = false;
        isPlayerTurn = false;
        sameTurn = false;
        selectedTile = null;
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
        List<Vector3> tilePosList = GameplayManager.instance.GetPossibleTilePos(token.name);
        List<GameObject> availableTiles = new List<GameObject>();
        for (int i = 0; i < tilePosList.Count; i++)
        {
            availableTiles.Add(GetTileAtPosition(tilePosList[i]));
        }
        foreach (GameObject tile in availableTiles)
        {
            tile.GetComponent<ToggleIndicators>().ToggleHighlight(true);
        }
    }

    public void UnhighlightAvailableTiles(GameObject token, string side)
    {
        List<Vector3> tilePosList = GameplayManager.instance.GetPossibleTilePos(token.name);
        List<GameObject> availableTiles = new List<GameObject>();
        for (int i = 0; i < tilePosList.Count; i++)
        {
            availableTiles.Add(GetTileAtPosition(tilePosList[i]));
        }
        foreach (GameObject tile in availableTiles)
        {
            tile.GetComponent<ToggleIndicators>().ToggleHighlight(false);
        }
    }

    private void ResetSelection()
    {
        SelectionManager selectionManager = SelectionManager.instance;
        selectionManager.selectionMode = SelectionManager.SelectionMode.Token;
        selectionManager.ClearSelectedToken();
    }

    private void OnTileClicked(GameObject tile, bool physicallyClicked)
    {
        if (!tiles.Contains(tile)) { return; }
        ColliderManager.instance.SwitchToTilesDeactivated();
        ColliderManager.instance.SwitchToTokensActivated();
        selectedTile = tile;
        DeactivateTileSelection();
        ResetSelection();

        if (physicallyClicked)
        {
            SelectionManager.instance.ForceTimeUp();
        }
    }
}
