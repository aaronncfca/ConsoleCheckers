using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleCheckers
{
    class Program
    {
        static void Main(string[] args)
        {
            game = new Game();
            consoleView = new ConsoleView();

            consoleView.MoveRequested += HandleMoveRequested;

            consoleView.ShowBoard(game.GetState());

            while (true)
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

        private static Game game;
        private static ConsoleView consoleView;
    }
}
