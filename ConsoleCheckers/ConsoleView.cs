using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleCheckers
{
    public class ConsoleView
    {
        public ConsoleView()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
        }

        public event EventHandler<MoveRequestedEventArgs> MoveRequested;

        public class MoveRequestedEventArgs
        {
            public int h;
            public int v;
            public Direction direction;
        }

        /// <summary>
        /// Attempts to read a move from console input. Displays a help message as needed.
        /// </summary>
        /// <param name="h"></param>
        /// <param name="v"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool GetInput(State state)
        {
            if (state.PieceMustJump != null)
            {
                Console.WriteLine($"Piece at { (char)(state.PieceMustJump.Item1 + 'A' - 1) }{(char)(state.PieceMustJump.Item2 + '1' - 1)} " +
                        $"can jump again!");
            }

            while (true)
            {
                string str = RequestInput(state.IsBlackTurn, false);

                if (str == "EXIT") return false;

                bool success = true;
                Direction direction = Direction.Northeast;
                int h = 0, v = 0;

                // If in a double-jump, give the user the option of specifying only the
                // direction to move.
                if (state.PieceMustJump != null && str.Length == 2)
                {
                    h = state.PieceMustJump.Item1;
                    v = state.PieceMustJump.Item2;
                }
                else
                {
                    if (str.Length != 5 || str[2] != ' ')
                    {
                        success = false;
                    }
                    else
                    {
                        h = str[0] - 'A' + 1;
                        v = str[1] - '1' + 1;

                        str = str.Substring(3);
                    }
                }

                if(success)
                {
                    switch (str)
                    {
                        case "NW":
                            direction = Direction.Northwest;
                            break;
                        case "NE":
                            direction = Direction.Northeast;
                            break;
                        case "SW":
                            direction = Direction.Southwest;
                            break;
                        case "SE":
                            direction = Direction.Southeast;
                            break;
                        default:
                            success = false;
                            break;
                    }
                }

                if (!success)
                {
                    Console.WriteLine("Invalid input. Type 'help' or '?' for help.");
                    continue;
                }

                var e = new MoveRequestedEventArgs() { h = h, v = v, direction = direction };

                if (MoveRequested != null)
                {
                    MoveRequested(this, e);
                }
                else
                {
                    throw new Exception("Someone ought to be listening."); // Should never happen.
                }

                return true;
            }
        }

        private string RequestInput(bool blackTurn, bool isDblJump)
        {
            while (true)
            {
                var temp = new State.SquareState();
                if (blackTurn)
                {
                    temp.Piece = State.Piece.Black;
                }
                else
                {
                    temp.Piece = State.Piece.White;
                }
                Console.Write($"{ StrSquare(temp) } > ");

                string str = Console.ReadLine().Trim().ToUpper();

                if (str == "HELP" || str == "?")
                {
                    if (!isDblJump)
                    {
                        Console.WriteLine("Please type the desired move by indicating the location of the piece followed by the direction to move it.");
                        Console.WriteLine("For example: A1 SE");
                    }
                    else
                    {
                        Console.WriteLine("Please type the desired direction to jump. For example: SE");
                    }
                    continue;
                }

                return str;
            }
        }

        public void ShowError(string error)
        {
            Console.WriteLine(error);
        }

        public void ShowBoard(State state)
        {
            /* 
             * NW   A  B  C  D  E  F  G  H   NE
             *    ┌────────────────────────┐
             *  1 │(O)   (O)   (O)   (O)   │
             *  2 │   (O)   (O)   (O)   (O)│  
             *  3 │(O)   (O)   (O)   (O)   │
             *  4 │                        │
             *  5 │                        │
             *  6 │   [□]   [□]   [□]   [□]│   Kings: [K]  (K)
             *  7 │[□]   [□]   [□]   [□]   │
             *  8 │   [□]   [□]   [□]   [□]│
             *    └────────────────────────┘
             * SW                            SE
             *    
             */

            Console.WriteLine();
            Console.WriteLine("NW   A  B  C  D  E  F  G  H   NE");
            Console.WriteLine("   ┌────────────────────────┐   ");
            Console.Write(StrState(state, true, ""));
            Console.WriteLine("   └────────────────────────┘   ");
            Console.WriteLine("SW                            SE");
            Console.WriteLine();
        }

        private string StrState(State state, bool withCoords, string indent)
        {
            var str = new StringBuilder();
            for (int v = 1; v <= 8; v++)
            {
                if (withCoords) str.Append($" {v} │");

                for(int h = 1; h <= 8; h++)
                {
                    str.Append(StrSquare(state[h, v]));
                }
                
                if(withCoords) str.Append($"│   ");
                str.Append(Environment.NewLine);
            }

            return str.ToString();
        }
        
        public string StrSquare(State.SquareState square)
        {
            switch (square.Piece)
            {
                case State.Piece.Black:
                    return "[\u25A1]";
                case State.Piece.White:
                    return "(O)";
                case State.Piece.BlackKing:
                    return "[K]";
                case State.Piece.WhiteKing:
                    return "(K)";
                case State.Piece.Empty:
                    return "   ";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
