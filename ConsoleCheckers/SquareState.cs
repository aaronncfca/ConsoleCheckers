namespace ConsoleCheckers
{
    public class SquareState
    {
        public SquareState() : this(State.PieceType.Empty) { }

        public SquareState(State.PieceType p)
        {
            PieceType = p;
        }

        public State.PieceType PieceType { get; set; }

        public bool IsEmpty()
        {
            return (PieceType == State.PieceType.Empty);
        }
    }
}
