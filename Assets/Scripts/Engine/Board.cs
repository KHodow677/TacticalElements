using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace GameplayEngine
{
    // Board class representing the game board
    public class Board
    {
        private Token[,] tiles; // 2D array representing the tiles on the board

        private Dictionary<string, Token> tokensByName;

        public Board()
        {
            tiles = new Token[5, 5];
            tokensByName = new Dictionary<string, Token>();
        }

        // Constructor that copies tokens from another board
        public Board(Board board)
        {
            tiles = new Token[5, 5];
            tokensByName = new Dictionary<string, Token>();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Token token = board.GetTokenAtPosition(i, j);
                    if (token != null)
                    {
                        Token copyToken = new Token(token.Name, token.X, token.Y, token.Player, token.MoveOffsets);
                        PlaceToken(copyToken, token.X, token.Y);
                    }
                }
            }
        }

        // Method to place a token on the board
        public void PlaceToken(Token token, int x, int y)
        {
            tiles[x, y] = token;
            token.X = x;
            token.Y = y;
            tokensByName[token.Name] = token; // Add the token to the tokensByName dictionary
        }

        // Method to move a token to a new position on the board
        public void MoveToken(Token token, int targetX, int targetY)
        {
            int currentX = token.X;
            int currentY = token.Y;

            // Clear the current position
            tiles[currentX, currentY] = null;

            // Check if there is a token at the target position
            Token capturedToken = tiles[targetX, targetY];
            if (capturedToken != null)
            {
                // Remove the captured token from the board
                tiles[targetX, targetY] = null;
            }

            // Place the token at the target position
            tiles[targetX, targetY] = token;

            // Update the token's position
            token.X = targetX;
            token.Y = targetY;
        }

        // Method to check if a position is valid (within the bounds of the board)
        public bool IsPositionValid(int x, int y)
        {
            return x >= 0 && x < 5 && y >= 0 && y < 5;
        }

        // Method to get the token at a specific position
        public Token GetTokenAtPosition(int x, int y)
        {
            return tiles[x, y];
        }

        // Method to find all legal moves for a specific player
        public List<(Token, (int, int))> FindAllLegalMoves(string player)
        {
            List<(Token, (int, int))> allLegalMoves = new List<(Token, (int, int))>();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Token token = tiles[i, j];
                    if (token != null && token.Player == player)
                    {
                        List<(int, int)> legalMoves = token.FindLegalMoves(this);
                        foreach ((int, int) move in legalMoves)
                        {
                            allLegalMoves.Add((token, move));
                        }
                    }
                }
            }

            return allLegalMoves;
        }

        // Method to perform the minimax algorithm with alpha-beta pruning and find the best move for a specific player
        public (Token, (int, int)) FindBestMove(string player, int depth)
        {
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            (Token, (int, int)) bestMove = (null, (0, 0));

            List<(Token, (int, int))> allLegalMoves = FindAllLegalMoves(player);

            foreach ((Token token, (int, int) move) in allLegalMoves)
            {
                // Perform the move on a copy of the board
                Board copyBoard = CreateCopy();
                copyBoard.MoveToken(token, move.Item1, move.Item2);

                // Calculate the minimax value for the opponent
                int value = Minimax(copyBoard, GetOpponentPlayer(player), alpha, beta, false, depth - 1);

                if (value > alpha)
                {
                    alpha = value;
                    bestMove = (token, move);
                }
            }

            return bestMove;
        }


        // Recursive method for the minimax algorithm with alpha-beta pruning
        public int Minimax(Board board, string player, int alpha, int beta, bool isMaximizingPlayer, int depth)
        {
            // Base case: Check if the game is over or depth limit is reached
            if (board.IsGameOver() || depth == 0)
            {
                return board.EvaluatePosition(player);
            }

            if (isMaximizingPlayer)
            {
                int maxEval = int.MinValue;

                List<(Token, (int, int))> allLegalMoves = board.FindAllLegalMoves(player);

                foreach ((Token token, (int, int) move) in allLegalMoves)
                {
                    // Perform the move on a copy of the board
                    Board copyBoard = board.CreateCopy();
                    copyBoard.PlaceToken(token, move.Item1, move.Item2);

                    int eval = Minimax(copyBoard, GetOpponentPlayer(player), alpha, beta, false, depth - 1);

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);

                    if (beta <= alpha)
                    {
                        break; // Beta cutoff
                    }
                }

                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                List<(Token, (int, int))> allLegalMoves = board.FindAllLegalMoves(player);

                foreach ((Token token, (int, int) move) in allLegalMoves)
                {
                    // Perform the move on a copy of the board
                    Board copyBoard = board.CreateCopy();
                    copyBoard.PlaceToken(token, move.Item1, move.Item2);

                    int eval = Minimax(copyBoard, GetOpponentPlayer(player), alpha, beta, true, depth - 1);

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);

                    if (beta <= alpha)
                    {
                        break; // Alpha cutoff
                    }
                }


                return minEval;
            }
        }


        // Method to check if the game is over (implement your own logic here)
        private bool IsGameOver()
        {
            // Add your game-over conditions here
            return false;
        }

        // Method to create a copy of the board
        public Board CreateCopy()
        {
            Board copyBoard = new Board();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Token token = tiles[i, j];
                    if (token != null)
                    {
                        Token copyToken = new Token(token.Name, token.X, token.Y, token.Player, token.MoveOffsets);
                        copyBoard.PlaceToken(copyToken, token.X, token.Y);
                    }
                }
            }
            return copyBoard;
        }

        // Method to get the opponent player for a given player
        private string GetOpponentPlayer(string player)
        {
            // Implement your own logic to determine the opponent player
            if (player == "Player 1")
            {
                return "Player 2";
            }
            else if (player == "Player 2")
            {
                return "Player 1";
            }
            else
            {
                return null;
            }
        }

        // Method to evaluate the position for a given player
        public int EvaluatePosition(string player)
        {
            int playerTokens = 0;
            int opponentTokens = 0;
            int positionalAdvantage = 0;

            // Define weights or scores for positional advantage
            int[,] positionWeights = new int[5, 5]
            {
                { 1, 2, 2, 2, 1 },
                { 2, 2, 2, 2, 2 },
                { 2, 2, 2, 2, 2 },
                { 2, 2, 2, 2, 2 },
                { 1, 2, 2, 2, 1 }
            };

            // Define token importance scores
            Dictionary<string, int> tokenScores = new Dictionary<string, int>()
            {
                { "Fire Token Variant", 3 },
                { "Ice Token Variant", 3 },
                { "Water Token Variant", 1 },
                { "Earth Token Variant", 1 },
                { "Land Storm Token Variant", 3 },
                { "Electricity Token Variant", 2 },
                { "Wind Token Variant", 2 },
                { "Plant Token Variant", 4 },
                { "Sea Storm Token Variant", 3 },
                { "Poison Token Variant", 4 },
                { "Light Token Variant", 5 },
                { "Dark Token Variant", 5 }
            };

            // Calculate the player and opponent tokens count and positional advantage
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Token token = tiles[i, j];
                    if (token != null)
                    {
                        if (token.Player == player)
                        {
                            playerTokens++;
                            positionalAdvantage += positionWeights[i, j];
                        }
                        else
                        {
                            opponentTokens++;
                            positionalAdvantage -= positionWeights[i, j];
                        }
                    }
                }
            }

            // Calculate the overall evaluation score
            int playerScore = playerTokens * 10;
            int opponentScore = opponentTokens * 10;
            int positionalScore = positionalAdvantage;
            int tokenImportanceScore = tokensByName.Values.Sum(t => tokenScores.GetValueOrDefault(t.Name, 0));

            // Return the sum of all evaluation components
            return playerScore + positionalScore + tokenImportanceScore - opponentScore;
        }
    }
}