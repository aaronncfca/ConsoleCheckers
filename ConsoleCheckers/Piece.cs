using System;

namespace ConsoleCheckers
{
    public class Piece
    {
        public Piece(int h, int v) : this(h, v, State.PieceType.Empty) { }

        public Piece(int h, int v, State.PieceType p)
        {
            PieceType = p;
            Coords = new Tuple<int, int>(h, v);
        }

        public bool IsEmpty()
        {
            return (PieceType == State.PieceType.Empty);
        }

        /// <summary>
        /// Move this piece to the given coordinates (update Coords property)
        /// 
        /// This method should only be called by the State class, which keeps track
        /// of where each Piece is.
        /// </summary>
        /// <param name="h"></param>
        /// <param name="v"></param>
        internal void Move(int h, int v)
        {
            Coords = new Tuple<int, int>(h, v);
        }

        public State.PieceType PieceType { get; set; }

        /// <summary>
        /// The coordinates of this piece on the board, where Item1 is h (horizontal, x),
        /// Item2 is v (vertical, y)
        /// </summary>
        public Tuple<int, int> Coords { get; private set; }
    }
}
