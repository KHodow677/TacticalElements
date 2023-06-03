using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour {
    private static SelectionManager _instance;
    public static SelectionManager instance { get { return _instance; } }
    
    [SerializeField] public float timePerTurn;
    [SerializeField] public float spaceHoldTimeToQuit;
    [HideInInspector] public float timeLeft;

    public enum GameMode { Draft, Gameplay };
    [HideInInspector] public GameMode gameMode = GameMode.Draft;
    // Events
    public delegate void KeyPressed();
    public delegate void TimeUp();
    public event KeyPressed spacePressed;
    public event TimeUp timeUp;
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
}
