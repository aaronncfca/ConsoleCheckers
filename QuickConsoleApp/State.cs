using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace QuickConsoleApp
{
    public class State
    {
        public class SquareState
        {
            public SquareState() : this(Piece.Empty) { }

            public SquareState(Piece p)
            {
                Piece = p;
            }

            public Piece Piece { get; set; }

            public bool IsEmpty()
            {
                return (Piece == Piece.Empty);
            }
        }

        public enum Piece
        {
            Empty = 0,
            White = PieceStandardBit,                   // 2 (yes, we skip the value 1)
            Black = PieceStandardBit | PieceBlackBit,   // 3
            WhiteKing = PieceKingBit,                   // 4
            BlackKing = PieceKingBit | PieceBlackBit    // 5
        }

        [Flags]
        public enum PieceFlags
        {
            Black = PieceBlackBit,
            Standard = PieceStandardBit,
            King = PieceKingBit
        }

        /// <summary>
        /// Bitwise indicator that a Piece is black. If the Piece is nonzero and the black
        /// bit is not set, then the Piece is white.
        /// </summary>
        public const int PieceBlackBit = 0b001;
        public const int PieceStandardBit = 0b010;
        public const int PieceKingBit = 0b100;


        public State()
        {
            board = new SquareState[8, 8];
            // Handled by SquareState initializer.
            for (int h = 0; h <= 7; h++)
            {
                for (int v = 0; v <= 7; v++)
                {
                    board[h, v] = new SquareState();
                }
            }
        }

        /// <summary>
        /// Initializes a copy of the given state.
        /// </summary>
        /// <param name="state">State to copy</param>
        public State(State state) : this()
        {
            for (int h = 0; h <= 7; h++)
            {
                for (int v = 0; v <= 7; v++)
                {
                    board[h, v].Piece = state.board[h, v].Piece;
                }
            }
        }

        public SquareState GetSquare(int h, int v)
        {
            return board[convertCoord(h), convertCoord(v)];
        }

        public void SetSquare(int h, int v, SquareState state)
        {
            board[convertCoord(h), convertCoord(v)] = state;
        }

        public void SetSquare(int h, int v, Piece piece)
        {
            SetSquare(h, v, new SquareState(piece));
        }

        public SquareState this[int h, int v]
        {
            get { return GetSquare(h, v); }
            set { SetSquare(h, v, value); }
        }

        private int convertCoord(int coordinate)
        {
            if(coordinate < 1 || coordinate > 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return coordinate - 1;
        }

        private SquareState[,] board;
    }
}
