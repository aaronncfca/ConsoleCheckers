using System;
using System.Data;

namespace ConsoleCheckers
{
    class Program
    {
        static void Main(string[] args)
        {
            game = new Game();
            consoleView = new ConsoleView();

            consoleView.MoveRequested += HandleMoveRequested;
            consoleView.PassRequested += HandlePassRequested;
            game.GameEnded += HandleGameEnded;

            consoleView.ShowBoard(game.GetState());

            while (!GameOver)
            {
                if (!consoleView.GetInput(game.GetState())) return;
            }
        }

        private static void HandleMoveRequested(object sender, ConsoleView.MoveRequestedEventArgs e)
        {
            try
            {
                game.Move(e.h, e.v, e.direction, out int h1, out int v1, out bool canDblJump);

                consoleView.ShowBoard(game.GetState());
            }
            catch (RuleBrokenException ex)
            {
                consoleView.ShowError(ex.Message);
            }

        }

        private static void HandlePassRequested(object sender, EventArgs e)
        {
            game.Pass();
            consoleView.ShowBoard(game.GetState());
        }

        private static void HandleGameEnded(object sender, Game.GameEndedEventArgs e)
        {
            consoleView.ShowGameEnded(game.GetState(), e.WinnerIsBlack);
            GameOver = true;
        }

        private static Game game;
        private static bool GameOver = false;
        private static ConsoleView consoleView;
    }
}