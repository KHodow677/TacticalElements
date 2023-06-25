using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour {
    private static SelectionManager _instance;
    public static SelectionManager instance { get { return _instance; } }

    [SerializeField] private SceneFader sceneFader;
    [SerializeField] private GameObject tokenDisplayObject;
    [SerializeField] private float scaleSpeed;

    [SerializeField] public float timePerTurn;
    [SerializeField] public float spaceHoldTimeToQuit;
    [SerializeField] public float timeLeft;

    public enum GameMode { Draft, Gameplay, GameOver};
    public enum SelectionMode { Token, Tile };
    public enum PlayerTurn { Player1, Player2 }
    [HideInInspector] public GameMode gameMode = GameMode.Draft;
    [HideInInspector] public SelectionMode selectionMode = SelectionMode.Token;
    [HideInInspector] public PlayerTurn playerTurn = PlayerTurn.Player1;
    [HideInInspector] public bool isGameplay;
    [HideInInspector] public bool tileIsClicked;

    // Events
    public delegate void ObjSelected(GameObject token);
    public delegate void ObjClicked(GameObject token, bool physicallyClicked);
    public delegate void TimeUp();
    public delegate void GameModeChanged();

    public event ObjClicked tokenClicked;
    public event ObjClicked tileClicked;
    public event ObjSelected tokenHovered;
    public event ObjSelected tokenUnhovered;
    public event TimeUp timeUp;
    public event GameModeChanged gameModeChanged;
    private bool clockPaused;
    private GameObject selectedToken;

    private void Awake() {
        // Ensure only one instance of the class exists
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
            return;
        }
        else { _instance = this; }
        timeLeft = timePerTurn;
    }

    private void Update() {
        // Call events to subscribers if conditions are met
        if (timeLeft <= 0) { timeUp?.Invoke(); timeLeft = timePerTurn; }
        if (!isGameplay && gameMode == GameMode.Gameplay) { gameModeChanged?.Invoke(); isGameplay = true; }

        if (clockPaused) { return; }
        timeLeft -= Time.deltaTime;
    }

    public void OnTokenSelect(GameObject token)
    {
        if (selectionMode != SelectionMode.Token) { return; }
        ScaleObject tokenScaler = token.GetComponent<ScaleObject>();
        TokenState tokenState = token.GetComponent<TokenState>();

        tokenScaler.ScaleUp(scaleSpeed);
        HandleDisplayToken(true, token);
        tokenHovered?.Invoke(token);
    }

    public void OnTokenDeselect(GameObject token)
    {
        if (selectionMode != SelectionMode.Token) { return; }
        ScaleObject tokenScaler = token.GetComponent<ScaleObject>();
        TokenState tokenState = token.GetComponent<TokenState>();

        tokenScaler.ScaleDown(scaleSpeed);
        HandleDisplayToken(false, token);
        tokenUnhovered?.Invoke(token);
    }

    public void OnTokenClicked(GameObject token, bool physicallyClicked)
    {
        if (selectionMode != SelectionMode.Token) { return; }
        if (playerTurn != PlayerTurn.Player1) { return; }
        tokenClicked?.Invoke(token, physicallyClicked);
    }

    public void OnTileSelect(GameObject tile)
    {
        if (selectionMode != SelectionMode.Tile) { return; }
        ScaleObject tileScaler = tile.GetComponent<ScaleObject>();
        ToggleIndicators tileIndicator = tile.GetComponent<ToggleIndicators>();

        tileScaler.ScaleUp(scaleSpeed);
        tileIndicator.ToggleTarget(true);        
    }

    public void OnTileDeselect(GameObject tile)
    {
        if (selectionMode != SelectionMode.Tile) { return; }
        ScaleObject tileScaler = tile.GetComponent<ScaleObject>();
        ToggleIndicators tileIndicator = tile.GetComponent<ToggleIndicators>();

        tileScaler.ScaleDown(scaleSpeed);
        tileIndicator.ToggleTarget(false);
    }

    public void OnTileClicked(GameObject tile, bool physicallyClicked)
    {
        if (selectionMode != SelectionMode.Tile) { return; }
        tileClicked?.Invoke(tile, physicallyClicked);
    }
    public void SetSelectedToken(GameObject token)
    {
        selectedToken = token;
    }

    public void ClearSelectedToken()
    {
        OnTokenDeselect(selectedToken);
        selectedToken = null;
    }

    private void HandleDisplayToken(bool isActivating, GameObject token)
    {
        GameObject tokenDisplay = tokenDisplayObject.transform.Find(token.gameObject.name).gameObject;

        if (isActivating)
        {
            tokenDisplay.SetActive(true);
            tokenDisplay.transform.GetChild(tokenDisplay.transform.childCount - 1).GetChild(0).gameObject.SetActive(true);
            tokenDisplay.GetComponent<ScaleObject>().ScaleUp(scaleSpeed);
        }
        else
        {
            tokenDisplay.GetComponent<ScaleObject>().ScaleDown(scaleSpeed);
            tokenDisplay.transform.GetChild(tokenDisplay.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
            SetInactiveDelayed(tokenDisplay);
        }
    }
    private async void SetInactiveDelayed(GameObject obj)
    {
        await Task.Delay((int)(scaleSpeed * 1000));
        obj.SetActive(false);
    }

    public void ForceTimeUp(){
        timeUp?.Invoke();
        timeLeft = timePerTurn;
        UnpauseClock();
    }

    public void PauseClock(){
        clockPaused = true;
    }
    public void UnpauseClock(){
        clockPaused = false;
    }

    public void CheckSubscribers(){
        Delegate[] eventHandlers = timeUp.GetInvocationList();
        for (int i = 0; i < eventHandlers.Length; i++)
        {
            MethodInfo methodInfo = eventHandlers[i].Method;
            Debug.Log("Method name: " + methodInfo.Name);
            Debug.Log("Declaring type: " + methodInfo.DeclaringType);
        }
    }
}
