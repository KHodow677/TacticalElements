using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DraftTokenSelection : MonoBehaviour
{
    [SerializeField] private DraftTileSelection draftTileSelection;
    [SerializeField] private EnemyDraftSelection enemyDraftSelection;
    [SerializeField] private GameObject tokenDisplayObject;
    [SerializeField] private float tokenScaleSpeed;
    [SerializeField] private GameObject playerTokenParent;
    [SerializeField] public List<GameObject> tokens;

    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isPlayerTurn = true;
    [HideInInspector] public GameObject selectedToken;

    [HideInInspector] public List<ScaleObject> tokenScalers;
    [HideInInspector] public List<TokenState> tokenStates;
    private ScaleObject selectedTokenScaler;
    private TokenState selectedTokenState; 

    private void Start()
    {
        InitializeTokenLists();
        ActivateTokenSelection();
        SelectionManager.instance.timeUp += SwitchStates;
    }

    private void InitializeTokenLists()
    {
        tokenScalers = new List<ScaleObject>();
        tokenStates = new List<TokenState>();

        foreach (var token in tokens)
        {
            tokenScalers.Add(token.GetComponent<ScaleObject>());
            tokenStates.Add(token.GetComponent<TokenState>());
        }
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
        SelectionManager.instance.spacePressed += OnSpacePressed;

        if (tokens.Count == 0)
        {
            EndDraft();
            return;
        }

        selectedTokenScaler = tokenScalers[0];
        selectedTokenState = tokenStates[0];
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        selectedTokenState.SetPlayerOwned();
        HandleDisplayToken(true);
        SetSameTurnDelayed();
        isSelecting = true;
    }

    private void EndDraft()
    {
        SelectionManager.instance.spacePressed -= OnSpacePressed;
        SelectionManager.instance.timeUp -= SwitchStates;
        SelectionManager.instance.timeUp -= draftTileSelection.SwitchStates;
        SelectionManager.instance.timeUp -= enemyDraftSelection.SwitchStates;
        SelectionManager.instance.PauseClock();
        SelectionManager.instance.gameMode = SelectionManager.GameMode.Gameplay;
    }

    private async void SetSameTurnDelayed()
    {
        await Task.Delay((int)(0.5f * SelectionManager.instance.timePerTurn * 1000));
        draftTileSelection.sameTurn = true;
    }

    private async void SetInactiveDelayed(GameObject obj)
    {
        await Task.Delay((int)(tokenScaleSpeed * 1000));
        obj.SetActive(false);
    }

    public void HandleDisplayToken(bool isActivating)
    {
        GameObject tokenDisplay = tokenDisplayObject.transform.Find(selectedTokenScaler.gameObject.name).gameObject;

        if (isActivating)
        {
            tokenDisplay.SetActive(true);
            tokenDisplay.transform.GetChild(tokenDisplay.transform.childCount - 1).GetChild(0).gameObject.SetActive(true);
            tokenDisplay.GetComponent<ScaleObject>().ScaleUp(tokenScaleSpeed);
        }
        else
        {
            tokenDisplay.GetComponent<ScaleObject>().ScaleDown(tokenScaleSpeed);
            tokenDisplay.transform.GetChild(tokenDisplay.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
            SetInactiveDelayed(tokenDisplay);
        }
    }

    private void DeactivateTokenSelection()
    {
        SelectionManager.instance.spacePressed -= OnSpacePressed;
        selectedToken = selectedTokenScaler.gameObject;
        selectedToken.transform.SetParent(playerTokenParent.transform);
        selectedToken.tag = "Player";
        RemoveSelectedToken();
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        isSelecting = false;
    }

    private void RemoveSelectedToken()
    {
        tokens.Remove(selectedToken);
        tokenScalers.Remove(selectedTokenScaler);
        tokenStates.Remove(selectedTokenState);
        enemyDraftSelection.tokens.Remove(selectedToken);
        enemyDraftSelection.tokenScalers.Remove(selectedTokenScaler);
    }

    private void OnSpacePressed()
    {
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        selectedTokenState.UnsetPlayerOwned();
        HandleDisplayToken(false);
        int currentIndex = tokenScalers.IndexOf(selectedTokenScaler);
        int nextIndex = (currentIndex + 1) % tokenScalers.Count;
        selectedTokenScaler = tokenScalers[nextIndex];
        selectedTokenState = tokenStates[nextIndex];
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        selectedTokenState.SetPlayerOwned();
        HandleDisplayToken(true);
    }
}
