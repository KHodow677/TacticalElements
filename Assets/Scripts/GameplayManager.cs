using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameplayEngine;
using System.Linq;
using System;
using System.Text;
using PlasticGui.WorkspaceWindow;

public class GameplayManager : MonoBehaviour
{
    private static GameplayManager _instance;
    public static GameplayManager instance { get { return _instance; } }

    private Engine gameplayEngine;

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform enemyTransform;

    [Header("Tile: Order is Top->Bottom Left -> Right")]
    [SerializeField] private List<Transform> allTileTransforms;

    private void Awake()
    {
        // Ensure only one instance of the class exists
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else { _instance = this; }
    }

    public void SetUpEngine()
    {
        // Step 1: Create tokens
        Token[] playerTokens = GetSideTokens(playerTransform);
        Token[] enemyTokens = GetSideTokens(enemyTransform);
        Token[] mergedArray = playerTokens.Concat(enemyTokens).ToArray();

        // Step 2: Create engine instance
        gameplayEngine = new Engine(mergedArray);

        // Step 3: Set up initial board state
        string initialBoardState = GenerateBoardState(mergedArray);
        gameplayEngine.HandleBoardState(initialBoardState);
    }

    public Token[] GetSideTokens(Transform sideTransform)
    {
        Token[] playerTokens = new Token[6];

        for (int i = 0; i < playerTokens.Length; i++)
        {
            // Get board coordinates
            (int, int) boardCoords = GetBoardCoordinates(sideTransform.GetChild(i).transform.position);

            // Generate Tokens to add
            TokenMoveOptions moveOptions = sideTransform.GetChild(i).GetComponent<TokenMoveOptions>();
            List<Vector3> moveOffsets = moveOptions.moveOffsetOptions;
            List<(int, int)> convertedOffsets = ConvertMoveOffsets(moveOffsets);

            string side = sideTransform == playerTransform ? "Player 1" : "Player 2";

            playerTokens[i] = new Token(
                sideTransform.GetChild(i).name,
                boardCoords.Item1,
                boardCoords.Item2,
                side,
                convertedOffsets
            );
        }

        return playerTokens;
    }
    private string GenerateBoardState(Token[] tokens)
    {
        StringBuilder boardStateBuilder = new StringBuilder();

        foreach (Token token in tokens)
        {
            boardStateBuilder.Append($"{token.Name}:{token.X},{token.Y};");
        }

        return boardStateBuilder.ToString().TrimEnd(';');
    }

    public (int, int) GetBoardCoordinates(Vector3 tilePosition)
    {
        // Board Parameters
        int numRows = 5;
        int numCols = 5;
        float tileSpacing = 1.6f;

        // X and Y coordinates of the leftmost tile
        float startX = -((numCols - 1) * tileSpacing) / 2f;
        float startY = ((numRows - 1) * tileSpacing) / 2f;

        // Calculate the relative position of the tile
        Vector3 relativePosition = tilePosition - new Vector3(startX, startY, 0f);

        // Calculate the column and row based on the relative position
        int column = Mathf.RoundToInt(relativePosition.x / tileSpacing);
        int row = Mathf.RoundToInt(-relativePosition.y / tileSpacing);

        return (column, row);
    }

    private List<(int, int)> ConvertMoveOffsets(List<Vector3> vectorOffsets)
    {
        List<(int, int)> moveOffsets = new List<(int, int)>();
        
        foreach (Vector3 vector in vectorOffsets)
        {
            moveOffsets.Add(((int) vector.x, (int) vector.y));
        }
        return moveOffsets;
    }

    public List<(int, int)> GetLegalMoves(string tokenName) => gameplayEngine.GetPossibleMoves(tokenName);
}
