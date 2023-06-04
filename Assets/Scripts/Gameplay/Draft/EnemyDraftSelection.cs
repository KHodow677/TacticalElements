using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDraftSelection : MonoBehaviour
{
    private static EnemyDraftSelection _instance;
    public static EnemyDraftSelection instance { get { return _instance; } }

    [SerializeField] private float timePerTurn;
    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isEnemyTurn = false;
    [SerializeField] private float tokenScaleSpeed;
    [SerializeField] private GameObject enemyTokenParent;
    [SerializeField] public List<GameObject> tokens;
    [SerializeField] public List<GameObject> tiles;
    [HideInInspector] public List<ScaleObject> tokenScalers;
    private ScaleObject selectedTokenScaler;
    [HideInInspector] public GameObject selectedToken;
    [HideInInspector] public GameObject selectedTile;

    private void Awake()
    {
        // Ensure only one instance of the class exists
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else { _instance = this; }
    }
    private void Start(){
        // Set up scaling components of tokens
        tokenScalers = new List<ScaleObject>();
        for (int i = 0; i < tokens.Count; i++) {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
        }
        SelectionManager.instance.timeUp += SwitchStates;
    }

    /// <summary>
    /// Switches between selecting and not selecting tokens.
    /// </summary>
    public void SwitchStates() {
        if (isSelecting) { SelectionManager.instance.ResetClock();}
        else if (isEnemyTurn) { StartCoroutine(SelectToken()); }
    }

    public IEnumerator SelectToken()
    {
        SelectionManager.instance.PauseClock();
        yield return new WaitForSeconds(timePerTurn);

        // Set up
        selectedTokenScaler = tokenScalers[UnityEngine.Random.Range(0, tokens.Count)];
        selectedTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
        selectedToken = selectedTokenScaler.gameObject;
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        TokenMoveController moveController = selectedToken.GetComponent<TokenMoveController>();
        moveController.StartMoveToPosition(selectedTile.transform.position);
        TokenMoveOptions tokenMoveOptions = selectedToken.GetComponent<TokenMoveOptions>();
        tokenMoveOptions.currentTile = selectedTile;
        isSelecting = true;

        // Set new parent for token
        selectedToken.transform.SetParent(enemyTokenParent.transform);
        selectedToken.tag = "Enemy";

        // Remove selected token from lists
        tokens.Remove(selectedToken);
        tokenScalers.Remove(selectedTokenScaler);
        tiles.Remove(selectedTile);
        DraftTokenSelection playerDraftManager = DraftTokenSelection.instance;
        playerDraftManager.tokens.Remove(selectedToken);
        playerDraftManager.tokenScalers.Remove(selectedTokenScaler);
        playerDraftManager.tokenStates.Remove(selectedToken.GetComponent<TokenState>());

        yield return new WaitForSeconds(moveController.moveDuration);
        SelectionManager.instance.UnpauseClock();

        // Tear down
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        isSelecting = false;
        isEnemyTurn = false;
        DraftTokenSelection.instance.isPlayerTurn = true;
        DraftTileSelection.instance.isPlayerTurn = true;
        DraftTileSelection.instance.sameTurn = false;
        SelectionManager.instance.ResetClock();
    }
}