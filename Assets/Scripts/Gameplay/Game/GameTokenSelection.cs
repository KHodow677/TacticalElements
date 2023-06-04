using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTokenSelection : MonoBehaviour {
    private static GameTokenSelection _instance;
    public static GameTokenSelection instance { get { return _instance; } }
    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isPlayerTurn = true;
    [SerializeField] private float tokenScaleSpeed;
    [SerializeField] public List<GameObject> tokens;
    [HideInInspector] public List<ScaleObject> tokenScalers;
    [HideInInspector] public List<TokenState> tokenStates;
    private ScaleObject selectedTokenScaler;
    private TokenState selectedTokenState;
    [HideInInspector] public GameObject selectedToken;

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
        // Force switch states
        SelectionManager.instance.timeUp += SwitchStates;
        SelectionManager.instance.ForceTimeUp();
        //SelectionManager.instance.CheckSubscribers();
    }

    /// <summary>
    /// Switches between selecting and not selecting tokens.
    /// </summary>
    public void SwitchStates() {
        if (isSelecting) { DeactivateTokenSelection(); }
        else if (isPlayerTurn) { ActivateTokenSelection(); }
    }

    /// <summary>
    /// Activates token selection.
    /// </summary>
    /// <param name="initialTileScaleSpeed">Initial scaling speed of token</param>
    private void ActivateTokenSelection() {
        // Add listener to spacePressed event
        SelectionManager.instance.spacePressed += OnSpacePressed;
        
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
        tokenStates = new List<TokenState>();
        for (int i = 0; i < tokens.Count; i++) {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
            tokenStates.Add(tokens[i].GetComponent<TokenState>());
        }

        // Sort tokens by their transform position
        tokens.Sort((obj1, obj2) =>
        {
            Vector3 pos1 = obj1.transform.position;
            Vector3 pos2 = obj2.transform.position;

            // Sort by ascending transform.position.y
            int yComparison = pos1.y.CompareTo(pos2.y);
            if (yComparison != 0) { return yComparison; }

            // For objects with the same transform.position.y, sort by ascending transform.position.x
            return pos1.x.CompareTo(pos2.x);
        });

        if(tokens.Count == 0) {
            Debug.Log("Tokens empty");
            SelectionManager.instance.spacePressed -= OnSpacePressed;
            SelectionManager.instance.PauseClock();
            SelectionManager.instance.gameMode = SelectionManager.GameMode.GameOver;
            return;
        }

        // Set up scaling and state components of tokens
        tokenScalers = new List<ScaleObject>();
        tokenStates = new List<TokenState>();
        for (int i = 0; i < tokens.Count; i++) {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
            tokenStates.Add(tokens[i].GetComponent<TokenState>());
        }

        // Set up
        selectedTokenScaler = tokenScalers[0];
        selectedTokenState = tokenStates[0];
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        selectedTokenState.SetPlayerOwned();
        StartCoroutine(SetSameTurnDelayed());
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
        // Tear down
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        isSelecting = false;
    }

    
    private IEnumerator SetSameTurnDelayed() {
        yield return new WaitForSeconds(0.5f * SelectionManager.instance.timePerTurn);
        GameTileSelection.instance.sameTurn = true;
        selectedToken = null;
    }

    /// <summary>
    /// Called when space is pressed.
    /// </summary>
    private void OnSpacePressed() {
        // Scale down and unhighlight current tile
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        selectedTokenState.UnsetPlayerOwned();

        // Get next tile, bounce back if end of list is reached
        int currentIndex = tokenScalers.IndexOf(selectedTokenScaler);
        int nextIndex = (currentIndex + 1) % tokenScalers.Count;
        selectedTokenScaler = tokenScalers[nextIndex];
        selectedTokenState = tokenStates[nextIndex];

        // Scale up next tile
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        selectedTokenState.SetPlayerOwned();
    }
}
