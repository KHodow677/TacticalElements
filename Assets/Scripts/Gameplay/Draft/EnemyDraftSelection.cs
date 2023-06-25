using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EnemyDraftSelection : MonoBehaviour
{
    [SerializeField] private DraftTokenSelection draftTokenSelection;
    [SerializeField] private DraftTileSelection draftTileSelection;

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
        if (isEnemyTurn) { SelectToken(); }
    }

    public async void SelectToken()
    {
        SelectionManager.instance.PauseClock();
        int delayMilliseconds = (int)(timePerTurn * 1000);
        await Task.Delay(delayMilliseconds);

        // Set up
        selectedToken = GetRandomElementWithBias(tokens);
        selectedTokenScaler = selectedToken.GetComponent<ScaleObject>();
        selectedTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];

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
        draftTokenSelection.tokens.Remove(selectedToken);

        delayMilliseconds = (int)(moveController.moveDuration * 1000);
        await Task.Delay(delayMilliseconds);
        SelectionManager.instance.UnpauseClock();

        // Tear down
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        isSelecting = false;
        isEnemyTurn = false;
        draftTokenSelection.isPlayerTurn = true;
        draftTileSelection.isPlayerTurn = true;
        draftTileSelection.sameTurn = false;
        DelaySetPlayerTurn(0.1f);
        SelectionManager.instance.ForceTimeUp();
    }

    private async void DelaySetPlayerTurn(float delaySeconds)
    {
        await Task.Delay((int) (delaySeconds * 1000));
        SelectionManager.instance.playerTurn = SelectionManager.PlayerTurn.Player1;
    }

    private T GetRandomElementWithBias<T>(List<T> list)
    {
        // Calculate weights based on the element index
        float[] weights = new float[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            weights[i] = (i + 1) * (i + 1); // You can adjust the weight calculation as per your desired bias
        }

        // Calculate the total weight
        float totalWeight = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            totalWeight += weights[i];
        }

        // Generate a random value between 0 and the total weight
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);

        // Iterate through the list and select an element based on the random value
        float weightSum = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            weightSum += weights[i];
            if (randomValue <= weightSum)
            {
                return list[i];
            }
        }

        // Return the last element if no selection was made (shouldn't happen in this case)
        return list[list.Count - 1];
    }
}
