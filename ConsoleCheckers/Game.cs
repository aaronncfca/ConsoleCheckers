using System;
using System.Collections.Generic;

namespace ConsoleCheckers
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

        public event EventHandler<GameEndedEventArgs> GameEnded;
        public class GameEndedEventArgs
        {
            public bool WinnerIsBlack;
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

            // Prepopulate the state history with the initial state, since the history is
            // updated at the end of each move, not at the beginning.
            stateHistory = new List<State> { new State(state) };
        }

        public State GetState()
        {
            return state;
        }

        /// <summary>
        /// Move the piece at h0, v0 in the given direction.
        /// 
        /// Throws RuleBrokenException if the move cannot be made.
        /// 
        /// If the given move is a jump and the same piece may make a second jump, canDblJmp will be
        /// set to true and the next Move should complete that jump unless Pass is invoked.
        /// </summary>
        /// <param name="h0">Horizontal coordinate of the piece to be moved</param>
        /// <param name="v0"></param>
        /// <param name="direction">The direction to move the piece</param>
        /// <param name="h1">Set to the new horizonal coordinate (x) of the piece, if moved.</param>
        /// <param name="v1">Set to the new vertical coordinate (y) of the piece, if moved.</param>
        /// <param name="canDblJmp">Set to true if the given piece jumped and can take a second jump.</param>
        public void Move(int h0, int v0, Direction direction, out int h1, out int v1, out bool canDblJmp)
        {
            canDblJmp = false;

            GetCoords(h0, v0, direction, out h1, out v1);

            bool isBlack = state.IsBlackTurn;

            bool canMove = CanMove(h0, v0, isBlack, ref h1, ref v1, out bool canJump, out string reason);

            // If we are in the middle of a double-jump, ensure the user has chosen the piece that is
            // jumping and has selected a move that is a jump.
            if (state.PieceMustJump != null)
            {
                if (state.PieceMustJump.Item1 != h0 || state.PieceMustJump.Item2 != v0 || !canJump)
                {
                    throw new RuleBrokenException($"Unable to move there: " +
                        $"piece at { (char)(state.PieceMustJump.Item1 + 'A' - 1) }{(char)(state.PieceMustJump.Item2 + '1' - 1)} " +
                        $"must jump again.");
                }
            }

            if (!canMove) throw new RuleBrokenException($"Unable to move there: { reason }");

            state[h1, v1] = state[h0, v0];

            int endRow = isBlack ? 1 : 8;
            if(v1 == endRow)
            {
                // This piece has reached the other side! It's a king.
                // Note that the piece may already be a king, but that's ok.
                state[h1, v1].PieceType = State.PieceType.King | (isBlack ? State.PieceType.Black : 0);
            }

            state.SetSquare(h0, v0, State.PieceType.Empty);

            if(canJump)
            {
                // Remove the jumped Piece.
                GetCoords(h0, v0, direction, out int hJumped, out int vJumped);
                state.SetSquare(hJumped, vJumped, State.PieceType.Empty);

                // Check whether a double jump is possible.
                CanMove(h1, v1, isBlack, out canDblJmp);
            }

            if(!canDblJmp)
            {
                state.IsBlackTurn = !isBlack;
                state.PieceMustJump = null;
            }
            else
            {
                state.PieceMustJump = new Tuple<int, int>(h1, v1);
            }

            stateHistory.Add(new State(state));

            if(CheckGameOver())
            {
                if(GameEnded != null)
                {
                    var e = new GameEndedEventArgs() { WinnerIsBlack = isBlack };
                    GameEnded(this, e);
                }
            }
        }


        /// <summary>
        /// Cede play to the other team.
        /// </summary>
        public void Pass()
        {
            state.IsBlackTurn = !state.IsBlackTurn;

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
                bool b = CanMove(h0, v0, isBlack, ref possibleMoves[i, 0], ref possibleMoves[i, 1], out canJump, out _);

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
                return false;
            }

            State.PieceType p0 = state[h0, v0].PieceType;

            if (p0 == State.PieceType.Empty)
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
            if (isBlack != p0.HasFlag(State.PieceType.Black))
            {
                reason = "It's not your turn.";
                return false;
            }

            if (!p0.HasFlag(State.PieceType.King))
            {
                // Piece is not a king.  Make sure it's being moved in the right direction.
                if (p0.HasFlag(State.PieceType.Black))
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

            State.PieceType p1 = state[h1, v1].PieceType;

            // Moving to an empty square. Go ahead!
            if (p1 == State.PieceType.Empty)
            {
                reason = "";
                return true;
            }


            if ((p0 & State.PieceType.Black) == (p1 & State.PieceType.Black))
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


                State.PieceType p2 = state[h2, v2].PieceType;

                if (p2 != State.PieceType.Empty)
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

        private bool CheckGameOver()
        {
            // This function is called after IsBlackTurn is switched; therefore we 
            // should check whether the upcoming player has any pieces on the board.
            // If not, game over.
            var playerPieces = state.GetPlayerPieces(state.IsBlackTurn);

            return playerPieces.Count == 0;
        }


        private Rules rules;
        private State state;
        private List<State> stateHistory;

        private const State.PieceType W = State.PieceType.Standard;
        private const State.PieceType B = State.PieceType.Standard | State.PieceType.Black;
        private static State.PieceType[,] standardStartArray => new State.PieceType[,] {
            { W, 0, W, 0, W, 0, W, 0 },
            { 0, W, 0, W, 0, W, 0, W },
            { W, 0, W, 0, W, 0, W, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, B, 0, B, 0, B, 0, B },
            { B, 0, B, 0, B, 0, B, 0 },
            { 0, B, 0, B, 0, B, 0, B }
        };
        private static State.PieceType[,] shortStartArray => new State.PieceType[,] {
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
