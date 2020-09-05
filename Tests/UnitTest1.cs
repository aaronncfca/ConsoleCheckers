using NUnit.Framework;
using ConsoleCheckers;
using System.Security.Cryptography;

namespace Tests
{
    public class Tests
    {
        private void HandleGameEnded(object sender, Game.GameEndedEventArgs e)
        {
            GameEndedTriggered++;
            GameEndedWinnerIsBlack = e.WinnerIsBlack;
        }

        private int GameEndedTriggered = 0;
        private bool GameEndedWinnerIsBlack;

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var game = new Game();
            var state = game.GetState();

            Assert.IsTrue(state[1, 1].PieceType == State.PieceType.Standard);
            Assert.IsTrue(state[8, 8].PieceType == (State.PieceType.Standard | State.PieceType.Black));
            Assert.IsTrue(state[1, 2].PieceType == State.PieceType.Empty);
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
            Assert.IsTrue(state[1, 1].PieceType == State.PieceType.Standard);
            Assert.IsTrue(state[3, 3].PieceType == State.PieceType.Standard);
            Assert.IsTrue(state[1, 3].PieceType == State.PieceType.Standard);
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
            Assert.IsTrue(state[1, 3].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[2, 4].PieceType == State.PieceType.Standard);

            game.Move(2, 6, Direction.Northeast, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state[2, 6].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[3, 5].PieceType == (State.PieceType.Standard | State.PieceType.Black));

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
            Assert.IsTrue(state[1, 3].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[2, 4].PieceType == State.PieceType.Standard);

            game.Move(4, 6, Direction.Northwest, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state[4, 6].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[3, 5].PieceType == (State.PieceType.Standard | State.PieceType.Black));

            game.Move(2, 4, Direction.Southeast, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state[2, 4].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[3, 5].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[4, 6].PieceType == State.PieceType.Standard);

            game.Move(5, 7, Direction.Northwest, out _, out _, out canDblJump);
            Assert.IsFalse(canDblJump);
            Assert.IsTrue(state[5, 7].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[4, 6].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[3, 5].PieceType == (State.PieceType.Standard | State.PieceType.Black));

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

            Assert.IsTrue(state[3, 5].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[2, 4].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[1, 3].PieceType == (State.PieceType.Standard | State.PieceType.Black));
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

            Assert.IsTrue(state[1, 3].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[2, 2].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[3, 1].PieceType == (State.PieceType.King | State.PieceType.Black));
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

            Assert.IsTrue(state[3, 1].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[4, 2].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[5, 3].PieceType == (State.PieceType.King | State.PieceType.Black));
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

            Assert.IsTrue(state[5, 3].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[4, 4].PieceType == State.PieceType.Empty);
            Assert.IsTrue(state[3, 5].PieceType == (State.PieceType.King | State.PieceType.Black));
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


        [Test]
        public void TryGameOver()
        {
            var game = new Game();
            var state = game.GetState();

            game.GameEnded += HandleGameEnded;

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
            game.Move(3, 5, Direction.Northwest, out _, out _, out _);
            game.Move(1, 3, Direction.Northeast, out _, out _, out _);
            game.Move(3, 1, Direction.Southeast, out _, out _, out _);
            game.Move(5, 3, Direction.Southwest, out _, out _, out _);

            /*
             * Game should look like this:
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
            game.Move(5, 1, Direction.Southwest, out _, out _, out _);
            game.Move(7, 5, Direction.Northwest, out _, out _, out _); // Double-jump, A
            game.Move(5, 3, Direction.Northwest, out _, out _, out _); // Double-jump, B, king me!
            game.Move(6, 2, Direction.Southwest, out _, out _, out _);
            game.Move(8, 4, Direction.Northwest, out _, out _, out _); // Jump
            game.Move(1, 1, Direction.Southeast, out _, out _, out _);
            game.Move(3, 1, Direction.Southwest, out _, out _, out _); // Jump backwards with a king
            game.Move(5, 3, Direction.Southwest, out _, out _, out _);
            game.Move(3, 5, Direction.Northeast, out _, out _, out _); // Jump with a king
            game.Move(3, 3, Direction.Southeast, out _, out _, out _);
            game.Move(5, 3, Direction.Southwest, out _, out _, out _); // Jump back with same king
            game.Move(8, 2, Direction.Southwest, out _, out _, out _); // 2 pieces left
            game.Move(6, 2, Direction.Northwest, out _, out _, out _); // King me!
            game.Move(7, 1, Direction.Southwest, out _, out _, out _);
            game.Pass();
            game.Move(7, 3, Direction.Southwest, out _, out _, out _);
            game.Move(5, 1, Direction.Southeast, out _, out _, out _); // Double-jump, A. One piece left now!

            Assert.IsTrue(GameEndedTriggered == 0);
            game.Move(7, 3, Direction.Southwest, out _, out _, out _); // Double-jump, B. Game over!
            Assert.IsTrue(GameEndedTriggered == 1);
        }
    }

}