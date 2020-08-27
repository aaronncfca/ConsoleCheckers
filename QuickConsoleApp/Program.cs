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
            var game = new Game();
            var state = game.GetState();
            var consoleView = new ConsoleView();
            bool blackTurn = false;

            consoleView.ShowBoard(state);

            while (true)
            {
                var temp = new State.SquareState(); // TODO: refactor out.
                if (blackTurn)
                {
                    temp.Piece = State.Piece.Black;
                }
                else
                {
                    temp.Piece = State.Piece.White;
                }
                Console.Write($"{ consoleView.StrSquare(temp) } > ");
                bool success = consoleView.TryGetMove(out int h, out int v, out Direction direction);
                if(!success)
                {
                    Console.WriteLine("Please try again. Type 'help' or '?' for help.");
                    continue;
                }

                try
                {
                    game.Move(h, v, blackTurn, direction, out int h1, out int v1, out bool canDblJump);
                    // TODO: check for double-jump if jumped.
                    while(canDblJump)
                    {
                        consoleView.GetInput(h1, v1);
                    }

                    blackTurn = !blackTurn;
                }
                catch(RuleBrokenException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                consoleView.ShowBoard(state);
            }
        }
    }
}
