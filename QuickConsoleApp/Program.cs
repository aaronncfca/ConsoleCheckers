using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            game = new Game();
            var state = game.GetState();
            consoleView = new ConsoleView();
            blackTurn = false;

            consoleView.MoveRequested += HandleMoveRequested;

            consoleView.ShowBoard(state);

            while (true)
            {
                consoleView.GetInput(blackTurn);

                consoleView.ShowBoard(state);
            }
        }
        private static void HandleMoveRequested(object sender, ConsoleView.MoveRequestedEventArgs e)
        {
            try
            {
                game.Move(e.h, e.v, blackTurn, e.direction, out int h1, out int v1, out bool canDblJump);

                if (canDblJump)
                {
                    consoleView.GetDoubleJump(h1, v1, blackTurn);
                    // TODO: this currently reults in recursion to this method with no way of
                    // retrying the double-jump if the user gives an invalid command (i.e.
                    // GetDoubleJump returns without the jump having been executed.)
                }
                else
                {
                    blackTurn = !blackTurn;
                }
            }
            catch (RuleBrokenException ex)
            {
                consoleView.ShowError(ex.Message);
            }

        }

        private static Game game;
        private static ConsoleView consoleView;
        private static bool blackTurn;
    }
}
