using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                    // Get the token at the new position
                    Token occupyingToken = board.GetTokenAtPosition(newX, newY);

                    // Check if the position is unoccupied or occupied by an opponent token
                    if (occupyingToken == null || occupyingToken.Player != Player)
                    {
                        legalMoves.Add((newX, newY));
                    }
                }
            }

            return legalMoves;
        }

    }
}