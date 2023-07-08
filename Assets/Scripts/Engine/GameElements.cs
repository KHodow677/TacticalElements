using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace GameplayEngine
{
    public class Token
    {
        public string Name { get; set; }
        public int X { get; set; } // current X position
        public int Y { get; set; } // current Y position
        public string Player { get; set; } // player who owns the token
        public List<(int, int)> MoveOffsets { get; set; } // list of offsets for legal moves

        public Token(string name, int x, int y, string player, List<(int, int)> moveOffsets)
        {
            Name = name;
            X = x;
            Y = y;
            Player = player;
            MoveOffsets = moveOffsets;
        }

        // Method to find legal moves for the token
        public List<(int, int)> FindLegalMoves(Board board)
        {
            List<(int, int)> legalMoves = new List<(int, int)>();
            foreach ((int offsetX, int offsetY) in MoveOffsets)
            {
                int newX = X + offsetX;
                int newY = Y + offsetY;

                // Check if the new position is within the bounds of the board
                if (board.IsPositionValid(newX, newY))
                {
                    legalMoves.Add((newX, newY));
                }
            }

            return legalMoves;
        }
    }

    // Board class representing the game board
    public class Board
    {
        private Token[,] tiles; // 2D array representing the tiles on the board

        public Board()
        {
            tiles = new Token[5, 5];
        }

        // Method to place a token on the board
        public void PlaceToken(Token token, int x, int y)
        {
            tiles[x, y] = token;
            token.X = x;
            token.Y = y;
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
                copyBoard.PlaceToken(token, move.Item1, move.Item2);

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
        private int Minimax(Board board, string player, int alpha, int beta, bool isMaximizingPlayer, int depth)
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
        private Board CreateCopy()
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

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Token token = tiles[i, j];
                    if (token != null)
                    {
                        if (token.Player == player)
                            playerTokens++;
                        else
                            opponentTokens++;
                    }
                }
            }

            // Return the difference between the number of player tokens and opponent tokens
            return playerTokens - opponentTokens;
        }
    }

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
                Console.WriteLine($"Token '{tokenName}' not found.");
                return;
            }

            // Parse the target position
            string[] positionParts = position.Split(',');
            int targetX = int.Parse(positionParts[0]);
            int targetY = int.Parse(positionParts[1]);

            // Check if the target position is a legal move for the token
            List<(int, int)> legalMoves = token.FindLegalMoves(board);
            if (!legalMoves.Contains((targetX, targetY)))
            {
                Console.WriteLine("Invalid move.");
                return;
            }

            // Move the token to the target position on the board
            board.PlaceToken(token, targetX, targetY);
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


        // Method to find a token on the board by its name
        private Token FindTokenByName(string tokenName)
        {
            if (tokensByName.TryGetValue(tokenName, out Token token))
            {
                return token;
            }

            return null;
        }

        // Method to get the possible moves for a given token on the board
        public List<(int, int)> GetPossibleMoves(string tokenName)
        {
            UnityEngine.Debug.Log(tokenName);
            if (tokensByName.TryGetValue(tokenName, out Token token))
            {
                UnityEngine.Debug.Log("Hi");
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

        // Method to find the best move for a player and perform it on the board
        public void PerformBestMove(string player, int depth)
        {
            (Token token, (int, int) move) = board.FindBestMove(player, depth);
            if (token == null)
            {
                Console.WriteLine($"No legal moves found for player '{player}'.");
                return;
            }

            int targetX = move.Item1;
            int targetY = move.Item2;

            board.PlaceToken(token, targetX, targetY);
        }
    }
}
