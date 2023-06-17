using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

public class GameTokenSelection : MonoBehaviour
{
    [SerializeField] private GameTileSelection gameTileSelection;
    [SerializeField] private EnemyGameSelection enemyGameSelection;
    [SerializeField] private GameObject tokenDisplayObject;
    [SerializeField] private SceneFader sceneFader;
    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isPlayerTurn = true;
    [SerializeField] private float tokenScaleSpeed;
    [SerializeField] public List<GameObject> tokens;
    [HideInInspector] public List<ScaleObject> tokenScalers;
    [HideInInspector] public List<TokenState> tokenStates;
    private ScaleObject selectedTokenScaler;
    private TokenState selectedTokenState;
    [HideInInspector] public GameObject selectedToken;

    private void Start()
    {
        SelectionManager.instance.gameModeChanged += OnGameplayMode;
        isSelecting = false;
    }

    private void OnGameplayMode()
    {
        SelectionManager.instance.timeUp += SwitchStates;
        SelectionManager.instance.ForceTimeUp();
    }

    public void SwitchStates()
    {
        if (isSelecting) { DeactivateTokenSelection(); }
        else if (isPlayerTurn) { ActivateTokenSelection(); }
    }

    private void ActivateTokenSelection()
    {
        SelectionManager.instance.spacePressed += OnSpacePressed;
        tokens = GetActivePlayerTokens();

        tokenScalers = new List<ScaleObject>();
        tokenStates = new List<TokenState>();
        InitializeTokenScalersAndStates();

        if (tokens.Count == 0)
        {
            GameOver();
            return;
        }

        selectedTokenScaler = tokenScalers[0];
        selectedTokenState = tokenStates[0];
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        gameTileSelection.HighlightAvailableTiles(selectedTokenScaler.gameObject, "Player");

        HandleDisplayToken(true);

        SetSameTurnDelayed();
        isSelecting = true;
    }
    private List<GameObject> GetActivePlayerTokens()
    {
        List<GameObject> activeTokens = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childObject = transform.GetChild(i).gameObject;
            if (!childObject.activeInHierarchy) { continue; }
            int tileCount = gameTileSelection.GetAvailableTiles(childObject, "Player").Count;
            if (tileCount == 0) { continue; }
            activeTokens.Add(childObject);
        }

        // Sort the tokens based on their positions
        activeTokens.Sort((obj1, obj2) =>
        {
            Vector3 pos1 = obj1.transform.position;
            Vector3 pos2 = obj2.transform.position;
            int yComparison = pos1.y.CompareTo(pos2.y);
            if (yComparison != 0) { return yComparison; }
            return pos1.x.CompareTo(pos2.x);
        });

        return activeTokens;
    }


    private void InitializeTokenScalersAndStates()
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            tokenScalers.Add(tokens[i].GetComponent<ScaleObject>());
            tokenStates.Add(tokens[i].GetComponent<TokenState>());
        }
    }

    private void GameOver()
    {
        SelectionManager.instance.spacePressed -= OnSpacePressed;
        SelectionManager.instance.PauseClock();
        SelectionManager.instance.gameMode = SelectionManager.GameMode.GameOver;
        StartCoroutine(sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, "Lose Scene"));
        SceneManager.LoadScene("Lose Scene");
    }

    private async void SetSameTurnDelayed()
    {
        await Task.Delay(TimeSpan.FromSeconds(0.5f * SelectionManager.instance.timePerTurn));
        gameTileSelection.sameTurn = true;
        selectedToken = null;
    }

    private async void SetInactiveDelayed(GameObject obj)
    {
        await Task.Delay(TimeSpan.FromSeconds(tokenScaleSpeed));
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
            return;
        }
        tokenDisplay.GetComponent<ScaleObject>().ScaleDown(tokenScaleSpeed);
        tokenDisplay.transform.GetChild(tokenDisplay.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
        SetInactiveDelayed(tokenDisplay);
    }

    private void DeactivateTokenSelection()
    {
        SelectionManager.instance.spacePressed -= OnSpacePressed;
        selectedToken = selectedTokenScaler.gameObject;
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        isSelecting = false;
    }

    private void OnSpacePressed()
    {
        selectedTokenScaler.ScaleDown(tokenScaleSpeed);
        gameTileSelection.UnhighlightAvailableTiles(selectedTokenScaler.gameObject, "Player");
        HandleDisplayToken(false);
        int currentIndex = tokenScalers.IndexOf(selectedTokenScaler);
        int nextIndex = (currentIndex + 1) % tokenScalers.Count;
        selectedTokenScaler = tokenScalers[nextIndex];
        selectedTokenState = tokenStates[nextIndex];
        selectedTokenScaler.ScaleUp(tokenScaleSpeed);
        gameTileSelection.HighlightAvailableTiles(selectedTokenScaler.gameObject, "Player");
        HandleDisplayToken(true);
    }
}
