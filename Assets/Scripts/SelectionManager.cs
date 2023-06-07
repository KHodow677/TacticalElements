using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour {
    private static SelectionManager _instance;
    public static SelectionManager instance { get { return _instance; } }

    [SerializeField] private SceneFader sceneFader;
    
    [SerializeField] public float timePerTurn;
    [SerializeField] public float spaceHoldTimeToQuit;
    [SerializeField] public float timeLeft;

    public enum GameMode { Draft, Gameplay, GameOver};
    [HideInInspector] public GameMode gameMode = GameMode.Draft;
    [HideInInspector] public bool isGameplay;
    // Events
    public delegate void KeyPressed();
    public delegate void TimeUp();
    public delegate void GameModeChanged();
    public event KeyPressed spacePressed;
    public event TimeUp timeUp;
    public event GameModeChanged gameModeChanged;
    private bool clockPaused;

    private void Awake() {
        // Ensure only one instance of the class exists
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
            return;
        }
        else { _instance = this; }
        timeLeft = timePerTurn;
    }

    private void Start() {
        // Add listener to spacePressed event
        spacePressed += OnSpaceHold;
    }

    private void Update() {
        // Call events to subscribers if conditions are met
        if (Input.GetKeyDown(KeyCode.Space)) { spacePressed?.Invoke(); }
        if (timeLeft <= 0) { timeUp?.Invoke(); timeLeft = timePerTurn; }
        if (!isGameplay && gameMode == GameMode.Gameplay) { gameModeChanged?.Invoke(); isGameplay = true; }

        if (clockPaused) { return; }
        timeLeft -= Time.deltaTime;
    }
    private void OnSpaceHold() {
        StartCoroutine(OnSpaceHoldQuit());
    }
    private IEnumerator OnSpaceHoldQuit() {
        float time = Time.time;
        while (Time.time - time < spaceHoldTimeToQuit) {
            if (!Input.GetKey(KeyCode.Space)) { yield break; }
            yield return null;
        }
        // Replace this with return to main menu!
        Debug.Log("Quitting");
        Application.Quit();
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
    public void ResetClock(){
        timeLeft = timePerTurn;
        timeUp?.Invoke();
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
