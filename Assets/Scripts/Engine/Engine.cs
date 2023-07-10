using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace GameplayEngine
{
    public class Engine
    {
        private Board board;
        private Dictionary<string, Token> tokensByName;

        public Engine(Token[] tokens)
        {
            board = new Board();
            tokensByName = new Dictionary<string, Token>();

            foreach (Token token in tokens)
            {
                tokensByName.Add(token.Name, token);
                board.PlaceToken(token, token.X, token.Y);
            }
        }

        public string GetTokenPlayer(string tokenName)
        {
            if (tokensByName.TryGetValue(tokenName, out Token token))
            {
                return token.Player;
            }
            // Token not found, return null or an appropriate default value
            return null;
        }

        public (int, int) GetTokenPosition(string tokenName)
        {
            if (tokensByName.TryGetValue(tokenName, out Token token))
            {
                return (token.X, token.Y);
            }

            // Token not found, return a default position
            return (-1, -1);
        }

        public string GetTokenNameFromMove(string move)
        {
            string[] moveParts = move.Split(':');
            if (moveParts.Length == 2)
            {
                return moveParts[0];
            }
            // Invalid move string format
            return null;
        }

        public (int, int) GetTargetPositionFromMove(string move)
        {
            string[] moveParts = move.Split(':');
            if (moveParts.Length == 2)
            {
                string position = moveParts[1];
                string[] positionParts = position.Split(',');

                if (positionParts.Length == 2 && int.TryParse(positionParts[0], out int targetX) && int.TryParse(positionParts[1], out int targetY))
                {
                    return (targetX, targetY);
                }
            }

            return (-1, -1);
        }

        // Method to handle a move input and update the board state
        public void HandleMove(string move)
        {
            // Parse the move input to extract the token name and the target position
            string[] moveParts = move.Split(':');
            string tokenName = moveParts[0];
            string position = moveParts[1];

            // Get the token from the dictionary using the token name
            if (!tokensByName.TryGetValue(tokenName, out Token token))
            {
                UnityEngine.Debug.Log($"Token '{tokenName}' not found.");
                return;
            }

            // Parse the target position
            string[] positionParts = position.Split(',');
            int targetX = int.Parse(positionParts[0]);
            int targetY = int.Parse(positionParts[1]);

            // Check if the target position is a legal move for the token
            List<(int, int)> legalMoves = token.FindLegalMoves(board);
            UnityEngine.Debug.Log(legalMoves.Count);
            if (!legalMoves.Contains((targetX, targetY)))
            {
                UnityEngine.Debug.Log("Invalid move.");
                return;
            }

            // Check if there is a token at the target position
            Token capturedToken = board.GetTokenAtPosition(targetX, targetY);
            if (capturedToken != null)
            {
                // Remove the captured token from the dictionary
                tokensByName.Remove(capturedToken.Name);
            }

            // Move the token to the target position on the board
            board.MoveToken(token, targetX, targetY);

            // Update the token's position in the tokensByName dictionary
            tokensByName[token.Name].X = targetX;
            tokensByName[token.Name].Y = targetY;
        }

        // Method to handle a board state input and update the board
        public void HandleBoardState(string boardState)
        {
            // Split the board state input into individual token positions
            string[] tokenPositions = boardState.Split(';');

            foreach (string tokenPosition in tokenPositions)
            {
                // Parse the token position
                string[] positionParts = tokenPosition.Split(':');
                string tokenName = positionParts[0];
                string[] position = positionParts[1].Split(',');

                int x = int.Parse(position[0]);
                int y = int.Parse(position[1]);

                // Find the corresponding token in the dictionary or create a new one
                Token token;
                if (tokensByName.TryGetValue(tokenName, out token))
                {
                    // Token already exists, update its position
                    token.X = x;
                    token.Y = y;
                }
                else
                {
                    // Token doesn't exist, create a new one and add it to the dictionary
                    token = new Token(tokenName, x, y, "Player 1", new List<(int, int)>());
                    tokensByName.Add(tokenName, token);
                }

                // Place the token on the board at the specified position
                board.PlaceToken(token, x, y);
            }
        }

        // Method to get the possible moves for a given token on the board
        public List<(int, int)> GetPossibleMoves(string tokenName)
        {
            if (tokensByName.TryGetValue(tokenName, out Token token))
            {
                return token.FindLegalMoves(board);
            }
            // Token not found, return an empty list
            return new List<(int, int)>(); 
        }

        // Method to get the current state of the board
        public string GetBoardState()
        {
            string boardState = "";

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Token token = board.GetTokenAtPosition(i, j);
                    if (token != null)
                    {
                        string tokenPosition = $"{token.Name}:{token.X},{token.Y};";
                        boardState += tokenPosition;
                    }
                }
            }

            return boardState.TrimEnd(';');
        }

        // Method to find the best move for a player and return it as a string
        public string GetBestMove(string player, int depth)
        {
            Board testBoard = board.CreateCopy();

            List<(Token, (int, int))> allLegalMoves = testBoard.FindAllLegalMoves(player);

            if (allLegalMoves.Count == 0)
            {
                UnityEngine.Debug.Log($"No legal moves found for player '{player}'.");
                return null;
            }

            (Token token, (int, int)) bestMove = allLegalMoves[0];
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            for (int i = 1; i < allLegalMoves.Count; i++)
            {
                (Token token, (int, int) move) = allLegalMoves[i];
                Board copyBoard = testBoard.CreateCopy();
                copyBoard.MoveToken(token, move.Item1, move.Item2);
                int value = Minimax(copyBoard, GetOpponentPlayer(player), alpha, beta, false, depth - 1);

                if (value > alpha)
                {
                    alpha = value;
                    bestMove = (token, move);
                }
            }

            int targetX = bestMove.Item2.Item1;
            int targetY = bestMove.Item2.Item2;

            return $"{bestMove.Item1.Name}:{targetX},{targetY}";
        }



    }
}
