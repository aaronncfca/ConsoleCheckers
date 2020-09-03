using NUnit.Framework;
using ConsoleCheckers;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var game = new Game();
            var state = game.GetState();

            Assert.IsTrue(state.GetSquare(1, 1).Piece == State.Piece.White);
            Assert.IsTrue(state.GetSquare(8, 8).Piece == State.Piece.Black);
            Assert.IsTrue(state.GetSquare(1, 2).Piece == State.Piece.Empty);
        }

        [Test]
        public void TryBadMoves()
        {

            var game = new Game();
            var state = game.GetState();

            state.IsBlackTurn = false; // Manually ensure white starts.

            try
            {
                game.Move(1, 1, Direction.Southwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                game.Move(1, 1, Direction.Southeast, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                game.Move(1, 1, Direction.Northwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                game.Move(1, 1, Direction.Northeast, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Move the wrong direction, into pieces
                game.Move(1, 3, Direction.Northeast, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Move off the board
                game.Move(1, 3, Direction.Southwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Space doesn't exist
                game.Move(2, 3, Direction.Southwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Empty space
                game.Move(4, 4, Direction.Southwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Wrong team
                game.Move(6, 6, Direction.Northwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            // Ensure pieces were not moved
            Assert.IsTrue(state.GetSquare(1, 1).Piece == State.Piece.White);
            Assert.IsTrue(state.GetSquare(3, 3).Piece == State.Piece.White);
            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.White);
        }


        [Test]
        public void TryBasicMoves()
        {

            var game = new Game();
            var state = game.GetState();
            bool canDblJump;

            state.IsBlackTurn = false; // Manually ensure white starts.

            game.Move(1, 3, Direction.Southeast, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(2, 4).Piece == State.Piece.White);

            game.Move(2, 6, Direction.Northeast, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state.GetSquare(2, 6).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Black);

            try
            {
                // Try moving piece backwards.
                game.Move(2, 4, Direction.Northwest, out _, out _, out canDblJump);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }
        }

        [Test]
        public void TryJump()
        {

            var game = new Game();
            var state = game.GetState();
            bool canDblJump;

            state.IsBlackTurn = false; // Manually ensure white starts.

            game.Move(1, 3, Direction.Southeast, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(2, 4).Piece == State.Piece.White);

            game.Move(4, 6, Direction.Northwest, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state.GetSquare(4, 6).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Black);

            game.Move(2, 4, Direction.Southeast, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state.GetSquare(2, 4).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(4, 6).Piece == State.Piece.White);

            game.Move(5, 7, Direction.Northwest, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state.GetSquare(5, 7).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(4, 6).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Black);

        }

        [Test]
        public void TryDoubleJump()
        {
            var game = new Game();
            var state = game.GetState();
            bool canDblJump;

            state.IsBlackTurn = true; // Manually ensure white starts.

            // Set up the board
            game.Move(6, 6, Direction.Northeast, out _, out _, out _);
            game.Move(1, 3, Direction.Southeast, out _, out _, out _);
            game.Move(4, 6, Direction.Northwest, out _, out _, out _);
            game.Move(5, 3, Direction.Southwest, out _, out _, out _);
            game.Move(5, 7, Direction.Northeast, out _, out _, out _);
            game.Move(4, 2, Direction.Southeast, out _, out _, out _);
            game.Move(7, 5, Direction.Northeast, out _, out _, out _);
            game.Move(5, 3, Direction.Southeast, out _, out _, out _);
            game.Move(6, 6, Direction.Northeast, out _, out _, out _);
            game.Move(3, 1, Direction.Southeast, out _, out _, out _);

            /*
             * Board should now look like this:
             * NW   A  B  C  D  E  F  G  H   NE
             *    ┌────────────────────────┐
             *  1 │(O)         (O)   (O)   │
             *  2 │   (O)   (O)   (O)   (O)│
             *  3 │      (O)         (O)   │
             *  4 │   (O)   (O)   (O)   [□]│
             *  5 │      [□]         [□]   │
             *  6 │   [□]               [□]│
             *  7 │[□]   [□]         [□]   │
             *  8 │   [□]   [□]   [□]   [□]│
             *    └────────────────────────┘
             * SW                            SE
             */


            Assert.IsTrue(state.IsBlackTurn);
            Assert.IsNull(state.PieceMustJump);

            game.Move(3, 5, Direction.Northwest, out _, out _, out canDblJump);

            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(2, 4).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.Black);
            Assert.IsTrue(canDblJump);
            Assert.IsNotNull(state.PieceMustJump);
            Assert.IsTrue(state.PieceMustJump.Item1 == 1);
            Assert.IsTrue(state.PieceMustJump.Item2 == 3);
            Assert.IsTrue(state.IsBlackTurn);


            try
            {
                // Try moving a different piece.
                game.Move(2, 6, Direction.Northwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            game.Move(1, 3, Direction.Northeast, out _, out _, out canDblJump);

            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(2, 2).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 1).Piece == State.Piece.BlackKing);
            Assert.IsTrue(canDblJump); // Should allow enforce triple jump.
            Assert.IsNotNull(state.PieceMustJump);
            Assert.IsTrue(state.PieceMustJump.Item1 == 3);
            Assert.IsTrue(state.PieceMustJump.Item2 == 1);
            Assert.IsTrue(state.IsBlackTurn);


            try
            {
                // Try moving to an empty space instead of jumping
                game.Move(3, 1, Direction.Southwest, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            game.Move(3, 1, Direction.Southeast, out _, out _, out canDblJump);

            Assert.IsTrue(state.GetSquare(3, 1).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(4, 2).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(5, 3).Piece == State.Piece.BlackKing);
            Assert.IsTrue(canDblJump); // Should allow enforce fourth jump.
            Assert.IsNotNull(state.PieceMustJump);
            Assert.IsTrue(state.PieceMustJump.Item1 == 5);
            Assert.IsTrue(state.PieceMustJump.Item2 == 3);
            Assert.IsTrue(state.IsBlackTurn);


            try
            {
                // Try jumping over a piece that is blocked on the other side.
                game.Move(5, 3, Direction.Southeast, out _, out _, out _);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            game.Move(5, 3, Direction.Southwest, out _, out _, out canDblJump);

            Assert.IsTrue(state.GetSquare(5, 3).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(4, 4).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.BlackKing);
            Assert.IsFalse(canDblJump); // No more jumps, boohoo. :'(
            Assert.IsNull(state.PieceMustJump);
            Assert.IsFalse(state.IsBlackTurn);

            /*
             * Final board should look like this:
             * NW   A  B  C  D  E  F  G  H   NE
             *    ┌────────────────────────┐
             *  1 │(O)         (O)   (O)   │
             *  2 │               (O)   (O)│
             *  3 │      (O)         (O)   │
             *  4 │               (O)   [□]│
             *  5 │      [K]         [□]   │
             *  6 │   [□]               [□]│
             *  7 │[□]   [□]         [□]   │
             *  8 │   [□]   [□]   [□]   [□]│
             *    └────────────────────────┘
             * SW                            SE
             */
        }
    }
}