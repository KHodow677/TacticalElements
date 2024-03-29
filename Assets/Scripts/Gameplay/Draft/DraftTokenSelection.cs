using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DraftTokenSelection : MonoBehaviour
{
    [SerializeField] private DraftTileSelection draftTileSelection;
    [SerializeField] private EnemyDraftSelection enemyDraftSelection;
    [SerializeField] private GameObject tokenDisplayObject;
    [SerializeField] private GameObject playerTokenParent;
    [SerializeField] public List<GameObject> tokens;
    [SerializeField] private float tokenScaleSpeed;

    [HideInInspector] public bool isSelecting; 
    [HideInInspector] public bool isPlayerTurn = true;
    [HideInInspector] public GameObject selectedToken;

    private void Start()
    {
        ActivateTokenSelection();
        SelectionManager.instance.timeUp += SwitchStates;
    }

    public void SwitchStates()
    {
        if (isSelecting)
        {
            DeactivateTokenSelection();
        }
        else if (isPlayerTurn)
        {
            ActivateTokenSelection();
        }
    }

    private void ActivateTokenSelection()
    {
        SelectionManager.instance.tokenClicked += OnTokenClicked;

        if (tokens.Count == 0)
        {
            EndDraft();
            return;
        }

        SetSameTurnDelayed(0.1f);
        isSelecting = true;
    }

    private void EndDraft()
    {
        SelectionManager selectionManager = SelectionManager.instance;
        selectionManager.tokenClicked -= OnTokenClicked;
        selectionManager.timeUp -= SwitchStates;
        selectionManager.timeUp -= draftTileSelection.SwitchStates;
        selectionManager.timeUp -= enemyDraftSelection.SwitchStates;
        selectionManager.PauseClock();
        selectionManager.gameMode = SelectionManager.GameMode.Gameplay;
    }

    private async void SetSameTurnDelayed(float delaySeconds)
    {
        await Task.Delay((int)(delaySeconds * 1000));
        draftTileSelection.sameTurn = true;
    }

    private void DeactivateTokenSelection()
    {
        if (selectedToken == null) 
        {
            selectedToken = tokens[Random.Range(0, tokens.Count)];
            SelectionManager.instance.OnTokenSelect(selectedToken);
            SelectionManager.instance.OnTokenClicked(selectedToken, false);
            return;
        }
        SelectionManager.instance.tokenClicked -= OnTokenClicked;
        selectedToken.transform.SetParent(playerTokenParent.transform);
        selectedToken.tag = "Player 1";
        tokens.Remove(selectedToken);
        enemyDraftSelection.tokens.Remove(selectedToken);
        selectedToken.GetComponent<TokenState>().SetPlayerOwned();

        isSelecting = false;
    }

    private void ResetSelection()
    {
        SelectionManager selectionManager = SelectionManager.instance;
        selectionManager.selectionMode = SelectionManager.SelectionMode.Tile;
        selectionManager.playerTurn = SelectionManager.PlayerTurn.Player2;
        selectionManager.SetSelectedToken(selectedToken);
        selectedToken.GetComponent<ScaleObject>().ScaleDown(tokenScaleSpeed);
    }

    private void OnTokenClicked(GameObject token, bool physicallyClicked)
    {
        if (!isSelecting) { return; }
        if (!tokens.Contains(token)) { return; }
        selectedToken = token;
        ColliderManager.instance.SwitchToTilesActivated();
        ColliderManager.instance.SwitchToTokensDeactivated(selectedToken);
        DeactivateTokenSelection();
        ResetSelection();
        if(physicallyClicked)
        {
            SelectionManager.instance.ForceTimeUp();
        }
    }
}
