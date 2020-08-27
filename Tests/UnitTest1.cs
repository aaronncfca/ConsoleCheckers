using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickConsoleApp;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var game = new Game();
            var state = game.GetState();

            Assert.IsTrue(state.GetSquare(1, 1).Piece == State.Piece.White);
            Assert.IsTrue(state.GetSquare(8, 8).Piece == State.Piece.Black);
            Assert.IsTrue(state.GetSquare(1, 2).Piece == State.Piece.Empty);
        }

        [TestMethod]
        public void TryBadMoves()
        {

            var game = new Game();
            var state = game.GetState();

            bool jumped;
            try
            {
                game.Move(1, 1, false, Direction.Southwest, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                game.Move(1, 1, false, Direction.Southeast, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                game.Move(1, 1, false, Direction.Northwest, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                game.Move(1, 1, false, Direction.Northeast, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Move the wrong direction, into pieces
                game.Move(1, 3, false, Direction.Northeast, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Move off the board
                game.Move(1, 3, false, Direction.Southwest, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Space doesn't exist
                game.Move(2, 3, false, Direction.Southwest, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Empty space
                game.Move(4, 4, false, Direction.Southwest, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            try
            {
                // Wrong team
                game.Move(3, 3, true, Direction.Southwest, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }

            // Ensure pieces were not moved
            Assert.IsTrue(state.GetSquare(1, 1).Piece == State.Piece.White);
            Assert.IsTrue(state.GetSquare(3, 3).Piece == State.Piece.White);
            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.White);
        }


        [TestMethod]
        public void TryBasicMoves()
        {

            var game = new Game();
            var state = game.GetState();
            bool jumped;

            game.Move(1, 3, false, Direction.Southeast, out jumped);
            Assert.IsFalse(jumped);
            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(2, 4).Piece == State.Piece.White);

            game.Move(2, 6, true, Direction.Northeast, out jumped);
            Assert.IsFalse(jumped);
            Assert.IsTrue(state.GetSquare(2, 6).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Black);

            try
            {
                // Try moving piece backwards.
                game.Move(2, 4, false, Direction.Northwest, out jumped);
                Assert.Fail();
            }
            catch (RuleBrokenException) { }
        }

        [TestMethod]
        public void TryJump()
        {

            var game = new Game();
            var state = game.GetState();
            bool jumped;

            game.Move(1, 3, false, Direction.Southeast, out jumped);
            Assert.IsFalse(jumped);
            Assert.IsTrue(state.GetSquare(1, 3).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(2, 4).Piece == State.Piece.White);

            game.Move(4, 6, true, Direction.Northwest, out jumped);
            Assert.IsFalse(jumped);
            Assert.IsTrue(state.GetSquare(4, 6).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Black);

            game.Move(2, 4, false, Direction.Southeast, out jumped);
            Assert.IsTrue(jumped);
            Assert.IsTrue(state.GetSquare(2, 4).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(4, 6).Piece == State.Piece.White);

            game.Move(5, 7, true, Direction.Northwest, out jumped);
            Assert.IsTrue(jumped);
            Assert.IsTrue(state.GetSquare(5, 7).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(4, 6).Piece == State.Piece.Empty);
            Assert.IsTrue(state.GetSquare(3, 5).Piece == State.Piece.Black);

        }
    }
}
