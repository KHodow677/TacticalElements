using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DraftTokenSelection : MonoBehaviour {

    [SerializeField] private DraftTileSelection draftTileSelection;
    [SerializeField] private EnemyDraftSelection enemyDraftSelection;

    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isPlayerTurn = true;
    [SerializeField] private GameObject tokenDisplayObject;
    [SerializeField] private float tokenScaleSpeed;
    [SerializeField] private GameObject playerTokenParent;
    [SerializeField] public List<GameObject> tokens;
    [HideInInspector] public List<ScaleObject> tokenScalers;
    [HideInInspector] public List<TokenState> tokenStates;
    private ScaleObject selectedTokenScaler;
    private TokenState selectedTokenState;
    [HideInInspector] public GameObject selectedToken;

    private void Start() {
        // Set up scaling and state components of tokens
        tokenScalers = new List<ScaleObject>();
        tokenStates = new List<TokenState>();
        for (int i = 0; i < tokens.Count; i++) {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
            tokenStates.Add(tokens[i].GetComponent<TokenState>());
        }
        ActivateTokenSelection();
        SelectionManager.instance.timeUp += SwitchStates;
    }

    /// <summary>
    /// Switches between selecting and not selecting tokens.
    /// </summary>
    public void SwitchStates() {
        if (isSelecting) { DeactivateTokenSelection(); }
        else if (isPlayerTurn) { ActivateTokenSelection(); }
    }

    /// <summary>
    /// Activates token selection
    /// </summary>
    /// <param name="initialTileScaleSpeed">Initial scaling speed of token</param>
    private void ActivateTokenSelection() {
        // Add listener to spacePressed event
        SelectionManager.instance.spacePressed += OnSpacePressed;

        if(tokens.Count == 0) {
            // End draft and tear down
            SelectionManager.instance.spacePressed -= OnSpacePressed;
            SelectionManager.instance.timeUp -= SwitchStates;
            SelectionManager.instance.timeUp -= draftTileSelection.SwitchStates;
            SelectionManager.instance.timeUp -= enemyDraftSelection.SwitchStates;
            SelectionManager.instance.PauseClock();
            SelectionManager.instance.gameMode = SelectionManager.GameMode.Gameplay;
            return;
        }

        // Set up
        selectedTokenScaler = tokenScalers[0];
        selectedTokenState = tokenStates[0];
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        selectedTokenState.SetPlayerOwned();
        HandleDisplayToken(true);
        SetSameTurnDelayed();
        isSelecting = true;
    }

    /// <summary>
    /// Deactivates token selection.
    /// </summary>
    /// <param name="initialTileScaleSpeed">Initial scaling speed of token</param>
    private void DeactivateTokenSelection() {
        // Remove listener to spacePressed event
        SelectionManager.instance.spacePressed -= OnSpacePressed;

        // Set selected token
        selectedToken = selectedTokenScaler.gameObject;

        // Set new parent for token
        selectedToken.transform.SetParent(playerTokenParent.transform);
        selectedToken.tag = "Player";

        // Remove selected token from lists
        tokens.Remove(selectedToken);
        tokenScalers.Remove(selectedTokenScaler);
        tokenStates.Remove(selectedTokenState);
        enemyDraftSelection.tokens.Remove(selectedToken);
        enemyDraftSelection.tokenScalers.Remove(selectedTokenScaler);

        // Tear down
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        isSelecting = false;
    }

    private async void SetSameTurnDelayed() {
        await Task.Delay((int)(0.5f * SelectionManager.instance.timePerTurn * 1000));
        draftTileSelection.sameTurn = true;
    }

    private async void SetInactiveDelayed(GameObject obj)
    {
        await Task.Delay((int)(tokenScaleSpeed * 1000));
        obj.SetActive(false);
    }

    public void HandleDisplayToken(bool isActivating) {
        GameObject tokenDisplay = tokenDisplayObject.transform.Find(selectedTokenScaler.gameObject.name).gameObject;
        if (isActivating) {
            tokenDisplay.SetActive(true);
            tokenDisplay.transform.GetChild(tokenDisplay.transform.childCount - 1).GetChild(0).gameObject.SetActive(true);
            tokenDisplay.GetComponent<ScaleObject>().ScaleUp(tokenScaleSpeed);
            return;
        }
        tokenDisplay.GetComponent<ScaleObject>().ScaleDown(tokenScaleSpeed);
        tokenDisplay.transform.GetChild(tokenDisplay.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
        SetInactiveDelayed(tokenDisplay);
    }

    /// <summary>
    /// Called when space is pressed.
    /// </summary>
    private void OnSpacePressed() {
        // Scale down and unhighlight current tile
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        selectedTokenState.UnsetPlayerOwned();
        HandleDisplayToken(false);

        // Get next tile, bounce back if end of list is reached
        int currentIndex = tokenScalers.IndexOf(selectedTokenScaler);
        int nextIndex = (currentIndex + 1) % tokenScalers.Count;
        selectedTokenScaler = tokenScalers[nextIndex];
        selectedTokenState = tokenStates[nextIndex];

        // Scale up next tile
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        selectedTokenState.SetPlayerOwned();
        HandleDisplayToken(true);
    }
}
