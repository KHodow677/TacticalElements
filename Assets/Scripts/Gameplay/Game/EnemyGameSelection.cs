using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class EnemyGameSelection : MonoBehaviour {

    [SerializeField] private GameTokenSelection gameTokenSelection;
    [SerializeField] private GameTileSelection gameTileSelection;

    [SerializeField] private SceneFader sceneFader;
    [SerializeField] private float timePerTurn;
    [HideInInspector] public bool isSelecting;
    [HideInInspector] public bool isEnemyTurn = false;

    [SerializeField] float moveDuration;

    private void Start() {
        SelectionManager.instance.gameModeChanged += OnGameplayMode;
        isSelecting = false;
    }

    private void OnGameplayMode() {
        // Subscribe to switch states
        SelectionManager.instance.timeUp += SwitchStates;
    }

    /// <summary>
    /// Switches between selecting and not selecting tiles.
    /// </summary>
    public void SwitchStates() {
        if (isEnemyTurn) { SelectAndMoveToken(); }
    }

    public async void SelectAndMoveToken() {
        SelectionManager.instance.PauseClock();
        await Task.Delay(TimeSpan.FromSeconds(timePerTurn));

        // Set up
        isSelecting = true;

        // Set up Player turn
        gameTokenSelection.isPlayerTurn = true;
        gameTileSelection.isPlayerTurn = true;
        gameTileSelection.sameTurn = false;
        isEnemyTurn = false;

        SelectionManager.instance.UnpauseClock();

        GameplayManager.instance.MakeBestMove("Player 2");

        await Task.Delay(TimeSpan.FromSeconds(moveDuration));

        // Tear down
        isSelecting = false;
        DelaySetPlayerTurn(0.1f);
        SelectionManager.instance.ForceTimeUp();
    }

    private async void DelaySetPlayerTurn(float delaySeconds)
    {
        await Task.Delay((int)(delaySeconds * 1000));
        SelectionManager.instance.playerTurn = SelectionManager.PlayerTurn.Player1;
    }
}
