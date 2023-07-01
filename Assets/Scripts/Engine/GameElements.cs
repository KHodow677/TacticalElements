using System;
using System.Collections.Generic;

class Token
{
    public string Name { get; set; }
    public int X { get; set; } // current X position
    public int Y { get; set; } // current Y position
    public string Player { get; set; } // player who owns the token

    public Token(string name, int x, int y, string player)
    {
        Name = name;
        X = x;
        Y = y;
        Player = player;
    }

    // Method to find legal moves for the token
    public List<(int, int)> FindLegalMoves(Board board)
    {
        List<(int, int)> legalMoves = new List<(int, int)>();

        // Add logic here to determine legal moves based on the token's movement pattern

        return legalMoves;
    }
}

// Board class representing the game board
class Board
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

    // Method to perform the minimax algorithm and find the best move for a specific player
    public (Token, (int, int)) FindBestMove(string player)
    {
        // Implement the minimax algorithm here to find the best move for the given player
        // You can use the FindAllLegalMoves method to generate the possible moves for the player

        // Placeholder code: return a random legal move for now
        List<(Token, (int, int))> allLegalMoves = FindAllLegalMoves(player);
        Random random = new Random();
        int randomIndex = random.Next(0, allLegalMoves.Count);
        return allLegalMoves[randomIndex];
    }
}
