using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
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
        /// Place a new SquareState at the given coordinates, initialized to the given
        /// Piece value.
        /// 
        /// This method should not be called to overwrite existing pieces.
        /// </summary>
        public void SetSquare(int h, int v, PieceType piece)
        {
            // Ensure this method is not called to remove pieces.
            if (this[h, v] != null && this[h, v].PieceType != PieceType.Empty) throw new ArgumentException();

            var newPiece = new SquareState(h, v, piece);
            
            // ConvertCoord will throw an exception if out of bounds.
            this[h, v] = newPiece;

            // If this is a new piece, add it to the list.
            if (piece != PieceType.Empty)
            {
                Pieces.Add(newPiece);
            }
        }

        /// <summary>
        /// Move the given piece to the given coordinates.
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="h"></param>
        /// <param name="v"></param>
        public void MovePiece(SquareState piece, int h, int v)
        {
            // Must move existing piece.
            if (piece.PieceType == PieceType.Empty) throw new ArgumentException();

            // Must move to empty square.
            if (this[h, v].PieceType != PieceType.Empty) throw new ArgumentException();

            int h0 = piece.Coords.Item1;
            int v0 = piece.Coords.Item2;

            // ConvertCoord will throw an exception if out of bounds.
            this[h, v] = piece;
            this[h0, v0] = new SquareState(h0, v0); // TODO: fill it with null?
            piece.Move(h, v);
        }


        public void RemovePiece(int h, int v)
        {
            SquareState piece = this[h, v];

            if (piece.PieceType == PieceType.Empty) throw new ArgumentException();

            bool success = Pieces.Remove(piece);

            if (!success) throw new ArgumentException();

            this[h, v] = new SquareState(h, v);
        }

        /// <summary>
        /// Get or set the SquareState at the given coordinates.
        /// 
        /// Throws ArgumentOutOfRangeException if a coordinate is invalid.
        /// 
        /// External callers must use MovePiece or RemovePiece instead of using
        /// this method to update the state.
        /// </summary>
        /// <param name="h">Horizontal (x) coordinate</param>
        /// <param name="v">Vertical (y) coordinate</param>
        /// <returns></returns>
        public SquareState this[int h, int v]
        {
            get { return Board[ConvertCoord(h), ConvertCoord(v)]; }
            private set { Board[ConvertCoord(h), ConvertCoord(v)] = value; } 
        }

        /// <summary>
        /// Gets a list of all the player's pieces remaining on the board.
        /// </summary>
        /// <param name="isBlack">True to check for black pieces, false for white.</param>
        /// <returns></returns>
        public List<SquareState> GetPlayerPieces(bool isBlack)
        {
            var playerPieces = from piece in Pieces
                               where (piece.PieceType.HasFlag(PieceType.Black) == isBlack)
                               select piece;

            return playerPieces.ToList();
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
