using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerDisplay : MonoBehaviour {
    [SerializeField] private Transform timerTransform;
    [SerializeField] private TMP_Text timerText;
    private float timerObjectScale;
    private void Start() {
        timerObjectScale = timerTransform.localScale.x;
    }
    private void Update() {
        // Update timer display
        float timeLeft = SelectionManager.instance.timeLeft;
        float timePerTurn = SelectionManager.instance.timePerTurn;
        timerTransform.localScale = new Vector3( timerObjectScale * timeLeft / timePerTurn, 0.5f, 1f);
        timerText.text = Mathf.Ceil(SelectionManager.instance.timeLeft).ToString();
    }
    /// <summary>
    /// Resets timer display to starting state when time is up
    /// </summary>
    private void OnTimesUp(){
        // Reset timer to starting state
        timerTransform.localScale = new Vector3(timerObjectScale, 0.5f, 1f);
        timerText.text = SelectionManager.instance.timePerTurn.ToString();
    }
}
