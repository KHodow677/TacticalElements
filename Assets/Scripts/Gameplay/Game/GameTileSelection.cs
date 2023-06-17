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
    private List<ScaleObject> tileScalers;
    private List<ToggleIndicators> tileIndicators;
    private ScaleObject selectedTileScaler;
    private ToggleIndicators selectedTileIndicator;
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
        SelectionManager.instance.spacePressed += OnSpacePressed;
        FindMoveOptionsFromSelectedToken();
    }

    private void DeactivateTileSelection()
    {
        SelectionManager.instance.spacePressed -= OnSpacePressed;
        gameTokenSelection.HandleDisplayToken(false);

        foreach (ToggleIndicators tileIndicator in tileIndicators)
        {
            tileIndicator.ToggleHighlight(false);
            tileIndicator.ToggleTarget(false);
        }

        selectedTile = selectedTileScaler.gameObject;
        GameObject selectedTokenObject = gameTokenSelection.selectedToken;
        TokenMoveController tokenMoveController = selectedTokenObject.GetComponent<TokenMoveController>();

        GameObject tokenAtTile = GetTokenAtPosition(selectedTile.transform.position, "Enemy");
        if (tokenAtTile != null) { tokenMoveController.StartMoveToPosition(selectedTile.transform.position, tokenAtTile); }
        else { tokenMoveController.StartMoveToPosition(selectedTile.transform.position); }

        selectedTileScaler.ScaleDown(tileScaleSpeed);
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
        tiles = GetAvailableTiles(selectedToken, "Player");
        tiles.Sort((obj1, obj2) =>
        {
            Vector3 pos1 = obj1.transform.position;
            Vector3 pos2 = obj2.transform.position;
            int yComparison = pos1.y.CompareTo(pos2.y);
            if (yComparison != 0) { return yComparison; }
            return pos1.x.CompareTo(pos2.x);
        });

        tileScalers = new List<ScaleObject>();
        tileIndicators = new List<ToggleIndicators>();
        for (int i = 0; i < tiles.Count; i++)
        {
            tileScalers.Add(tiles[i].GetComponent<ScaleObject>());
            tileIndicators.Add(tiles[i].GetComponent<ToggleIndicators>());
        }

        foreach (ToggleIndicators tileIndicator in tileIndicators)
        {
            tileIndicator.ToggleHighlight(true);
        }

        selectedTileScaler = tileScalers[0];
        selectedTileIndicator = tileIndicators[0];
        selectedTileScaler.ScaleUp(tileScaleSpeed);
        selectedTileIndicator.ToggleTarget(true);
        isSelecting = true;

        SetEnemyTurnDelayed();
        gameTokenSelection.isPlayerTurn = false;
        enemyGameSelection.isEnemyTurn = true;
        isPlayerTurn = false;
        sameTurn = false;
    }

    private async void SetEnemyTurnDelayed()
    {
        await Task.Delay(TimeSpan.FromSeconds(0.5f * SelectionManager.instance.timePerTurn));
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

    private void OnSpacePressed()
    {
        selectedTileScaler.ScaleDown(tileScaleSpeed);
        selectedTileIndicator.ToggleTarget(false);

        int currentIndex = tileScalers.IndexOf(selectedTileScaler);
        int nextIndex = (currentIndex + 1) % tileScalers.Count;
        selectedTileScaler = tileScalers[nextIndex];
        selectedTileIndicator = tileIndicators[nextIndex];
        selectedTileScaler.ScaleUp(tileScaleSpeed);
        selectedTileIndicator.ToggleTarget(true);
    }
}
