using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConsoleApp
{
    [System.Serializable]
    public class RuleBrokenException : Exception
    {
        public RuleBrokenException() { }
        public RuleBrokenException(string message) : base(message) { }
        public RuleBrokenException(string message, Exception inner) : base(message, inner) { }
        protected RuleBrokenException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Represents the direction of a move, where the "northwest" corner
    /// of the board is 1, 1 and the southeast corner is 8, 8. Black always
    /// starts on the south side of the board.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Horizontal +1, vertical +1
        /// </summary>
        Southeast,

        /// <summary>
        /// Horizontal +1, vertical -1
        /// </summary>
        Northeast,

        /// <summary>
        /// Horizontal -1, vertical +1
        /// </summary>
        Southwest,


        /// <summary>
        /// Horizontal -1, vertical -1
        /// </summary>
        Northwest
    }

    public class Game
    {

        public enum Rules
        {
            /// <summary>
            /// Standard rules and board setup.
            /// </summary>
            Normal,

            /// <summary>
            /// "Sparce" variation: players start with 8 pieces instead of 12.
            /// </summary>
            Sparce
        }

        public Game() : this(Rules.Normal) { }

        public Game(Rules r)
        {
            rules = r;
            state = new State();
            stateHistory = new List<State> { state };

            var startArray = (rules == Rules.Sparce) ? shortStartArray : standardStartArray;
            
            for(int v = 0; v < startArray.GetLength(0); v++)
            {
                for (int h = 0; h < startArray.GetLength(1); h++)
                {
                    // SetSquare takes coordinates from 1 to 8, column first.
                    state.SetSquare(h + 1, v + 1, startArray[v, h]);
                }
            }

            stateHistory = new List<State>();
        }

        public State GetState()
        {
            return state;
        }

        public void Move(int h0, int v0, bool isBlack, Direction direction, out int h1, out int v1, out bool canDblJmp)
        {
            canDblJmp = false;

            GetCoords(h0, v0, direction, out h1, out v1);

            bool canMove = CanMove(h0, v0, isBlack, ref h1, ref v1, out bool canJump, out string reason);

            if (!canMove) throw new RuleBrokenException($"Unable to move there: { reason }");

            state[h1, v1] = state[h0, v0];

            int endRow = isBlack ? 1 : 8;
            if(v1 == endRow)
            {
                // This piece has reached the other side! It's a king.
                // Note that the piece may already be a king, but that's ok.
                state[h1, v1].Piece = isBlack ? State.Piece.BlackKing : State.Piece.WhiteKing;
            }

            state.SetSquare(h0, v0, State.Piece.Empty);

            if(canJump)
            {
                GetCoords(h0, v0, direction, out int hJumped, out int vJumped);
                state.SetSquare(hJumped, vJumped, State.Piece.Empty);

                // Check whether a double jump is possible.
                CanMove(h1, v1, isBlack, out canDblJmp);
            }

            stateHistory.Add(new State(state));
        }

        private void GetCoords(int h0, int v0, Direction direction, out int h1, out int v1)
        {
            switch (direction)
            {
                case Direction.Southeast:
                    h1 = h0 + 1;
                    v1 = v0 + 1;
                    break;
                case Direction.Northeast:
                    h1 = h0 + 1;
                    v1 = v0 - 1;
                    break;
                case Direction.Southwest:
                    h1 = h0 - 1;
                    v1 = v0 + 1;
                    break;
                case Direction.Northwest:
                    h1 = h0 - 1;
                    v1 = v0 - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Checks whether the piece at the given coordinates can move.
        /// </summary>
        /// <param name="h0">Horizontal coordinate of piece to move.</param>
        /// <param name="v0">Vertical coordinate of piece to move.</param>
        /// <param name="isBlack">Whether it is currently black's turn (white's otherwise)</param>
        /// <param name="canJump">Set to true if this piece can jump anywhere, false otherwise.</param>
        /// <returns>True if this piece can move anywhere.</returns>
        private bool CanMove(int h0, int v0, bool isBlack, out bool canJump)
        {
            bool toReturn = false;
            canJump = false;

            int[,] possibleMoves = new int[,] {
                { h0 + 1, v0 + 1 },
                { h0 + 1, v0 - 1 },
                { h0 - 1, v0 + 1 },
                { h0 - 1, v0 - 1 } };

            for (int i = 0; i < 4; i++)
            {
                bool b = CanMove(h0, v0, isBlack, ref possibleMoves[i, 0], ref possibleMoves[i, 1], out canJump, _);

                // If we can jump here, return immediately. If we can move but not jump, keep checking
                // for a chance to jump.
                if (b && canJump) { return true; }
                if (b) { toReturn = true; }
            }

            return toReturn;
        }

        private bool CanMove(int h0, int v0, bool isBlack, ref int h1, ref int v1, out bool isJump, out string reason)
        {
            isJump = false;
            if (h0 < 1 || h0 > 8
                || v0 < 1 || v0 > 8 // Ensure coordinates fit on the board.
                || ((h0 + v0) & 0x1) == 0x1) // Ensure coordinats are either both even or both odd.
            {
                reason = "Invalid coordinates.";
                return false;
            }

            if (h1 < 1 || h1 > 8
                || v1 < 1 || v1 > 8)
            {
                reason = "Can't move off the board.";
            }

            State.Piece p0 = state[h0, v0].Piece;

            if (p0 == State.Piece.Empty)
            {
                reason = "No piece at given coordinates.";
                return false;
            }

            // Ensure move is to a diagnally adjacent square.
            // TODO: try implementing optional rules that allow for straight moves.
            if (    (v0 + 1 != v1 && v0 - 1 != v1)
                ||  (h0 + 1 != h1 && h0 - 1 != h1))
            {
                reason = "Invalid move.";
                return false;
            }

            // Ensure the piece is the color of the player whose turn it is.
            if (isBlack != (((int)p0 & State.PieceBlackBit) != 0))
            {
                reason = "It's not your turn.";
                return false;
            }

            if (((int)p0 & State.PieceKingBit) == 0)
            {
                // Piece is not a king.  Make sure it's being moved in the right direction.
                if (((int)p0 & State.PieceBlackBit) != 0)
                {
                    // Piece is black, heading north (towards lower values)
                    if (v1 > v0)
                    {
                        reason = "Can't move backwards.";
                        return false;
                    }
                }
                else
                {
                    // Piece is white, heading north (towards higher values)
                    if (v1 < v0)
                    {
                        reason = "Can't move backwards.";
                        return false;
                    }
                }
            }

            State.Piece p1 = state[h1, v1].Piece;

            // Moving to an empty square. Go ahead!
            if (p1 == State.Piece.Empty)
            {
                reason = "";
                return true;
            }


            if (((int)p0 & State.PieceBlackBit) == ((int)p1 & State.PieceBlackBit))
            {
                reason = "You have a piece in the way.";
                return false;
            }
            else
            {
                // Piece in the selected square is of the opponent. Let's see if we can jump it!
                int v2 = v1 * 2 - v0;
                int h2 = h1 * 2 - h0;

                if (h2 < 1 || h2 > 8
                    || v2 < 1 || v2 > 8)
                {
                    reason = "Unable to jump off the board.";
                    return false;
                }


                State.Piece p2 = state[h2, v2].Piece;

                if (p2 != State.Piece.Empty)
                {
                    reason = "Jump is blocked.";
                    return false;
                }

                isJump = true;
                v1 = v2;
                h1 = h2;
                reason = "";
                return true;
            }
        }


        private Rules rules;
        private State state;
        private List<State> stateHistory;

        private const State.Piece W = State.Piece.White;
        private const State.Piece B = State.Piece.Black;
        private static State.Piece[,] standardStartArray => new State.Piece[,] {
            { W, 0, W, 0, W, 0, W, 0 },
            { 0, W, 0, W, 0, W, 0, W },
            { W, 0, W, 0, W, 0, W, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, B, 0, B, 0, B, 0, B },
            { B, 0, B, 0, B, 0, B, 0 },
            { 0, B, 0, B, 0, B, 0, B }
        };
        private static State.Piece[,] shortStartArray => new State.Piece[,] {
            { W, 0, W, 0, W, 0, W, 0 },
            { 0, W, 0, W, 0, W, 0, W },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { B, 0, B, 0, B, 0, B, 0 },
            { 0, B, 0, B, 0, B, 0, B }
        };
    }
}
