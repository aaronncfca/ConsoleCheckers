using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleCheckers
{
    public class State
    {
        /// <summary>
        /// Used to store and test the type of piece in a given SquareState.
        /// </summary>
        [Flags]
        public enum PieceType
        {
            /// <summary>
            /// No piece exists here.
            /// </summary>
            Empty = 0,

            /// <summary>
            /// Set if this piece is black; clear if this piece is white or empty.
            /// </summary>
            Black = 1,

            /// <summary>
            /// Set if this piece is not empty unless it's a king.
            /// </summary>
            Standard = 2,

            /// <summary>
            /// Set if this piece is a king.
            /// </summary>
            King = 4
        }


        public State()
        {
            Board = new SquareState[8, 8];
            Pieces = new List<SquareState>();

            for (int h = 1; h <= 8; h++)
            {
                for (int v = 1; v <= 8; v++)
                {
                    SetSquare(h, v, PieceType.Empty);
                }
            }

            IsBlackTurn = true;
            PieceMustJump = null;
        }

        /// <summary>
        /// Initializes a copy of the given state.
        /// </summary>
        /// <param name="state">State to copy</param>
        public State(State state)
        {
            Board = new SquareState[8, 8];
            Pieces = new List<SquareState>();

            for (int h = 1; h <= 8; h++)
            {
                for (int v = 1; v <= 8; v++)
                {
                    // SetSquare will populate Board and Pieces for us.
                    SetSquare(h, v, state[h, v].PieceType);
                }
            }

            IsBlackTurn = state.IsBlackTurn;
            PieceMustJump = state.PieceMustJump;
        }

        /// <summary>
        /// Retrieve the SquareState at the given coordinates on the board.
        /// </summary>
        public SquareState GetSquare(int h, int v)
        {
            return Board[ConvertCoord(h), ConvertCoord(v)];
        }

        /// <summary>
        /// Place the given SquareState at the given coordinates on the board.
        /// </summary>
        public void SetSquare(int h, int v, SquareState state)
        {
            var oldPiece = GetSquare(h, v);

            // ConvertCoord will throw an exception if out of bounds.
            Board[ConvertCoord(h), ConvertCoord(v)] = state;
            state.Move(h, v);

            // If the given piece is replacing another one, remove the overwritten
            // piece from the list.
            if(oldPiece.PieceType != PieceType.Empty)
            {
                Pieces.Remove(oldPiece);
            }
        }

        /// <summary>
        /// Place a new SquareState at the given coordinates, initialized to the given
        /// Piece value.
        /// </summary>
        public void SetSquare(int h, int v, PieceType piece)
        {
            // The coords of the new SquareSpace will be set in SetSquare, so we use 1, 1 here.
            var newPiece = new SquareState(1, 1, piece);

            SetSquare(h, v, newPiece);

            // If this is a new piece, add it to the list.
            if(piece != PieceType.Empty)
            {
                Pieces.Add(newPiece);
            }
        }

        public SquareState this[int h, int v]
        {
            get { return GetSquare(h, v); }
            set { SetSquare(h, v, value); }
        }

        private int ConvertCoord(int coordinate)
        {
            if(coordinate < 1 || coordinate > 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return coordinate - 1;
        }

        /// <summary>
        /// True if it's black's turn to move or black is currently moving; false
        /// if white's. (Black = player starting at the south side of the board,
        /// v = 6 through 8.)
        /// </summary>
        public bool IsBlackTurn { get; set; }

        /// <summary>
        /// If not null, indicates that the piece at this coordinate must jump
        /// another piece before any other moves are made (this is an edge case
        /// caused by double-jumps).
        /// </summary>
        public Tuple<int, int> PieceMustJump { get; set; }

        private SquareState[,] Board;
        private List<SquareState> Pieces;
    }
}
