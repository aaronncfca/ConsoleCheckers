namespace ConsoleCheckers
{
    public class SquareState
    {
        public SquareState() : this(State.Piece.Empty) { }

        public SquareState(State.Piece p)
        {
            Piece = p;
        }

        public State.Piece Piece { get; set; }

        public bool IsEmpty()
        {
            return (Piece == State.Piece.Empty);
        }
    }
}
