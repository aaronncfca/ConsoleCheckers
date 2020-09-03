using System;
using System.Reflection;
using System.Text;

namespace ConsoleCheckers
{
    public class ConsoleView
    {
        public ConsoleView()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            Console.WriteLine($"Welcome to Console Checkers version { assemblyName.Version }!");
            Console.WriteLine();
            Console.WriteLine("To play, take turns specifying the coordinates of the piece that the player would like");
            Console.WriteLine("to move and the direction to move it. For example, the command \"A3 SE\" would move the");
            Console.WriteLine("the round piece on the left side downwards.");
            Console.WriteLine();
            Console.WriteLine("KNOWN ISSUE:");
            Console.WriteLine("    The game engine does not currently check for end-game scenarios or for scenarios");
            Console.WriteLine("    where a player cannot move. It will continue asking for a valid move even when there");
            Console.WriteLine("    is none.");
            Console.WriteLine();
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
                var temp = new SquareState();
                if (blackTurn)
                {
                    temp.PieceType = State.PieceType.Standard | State.PieceType.Black;
                }
                else
                {
                    temp.PieceType = State.PieceType.Standard;
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

        private string StrState(State state, bool withCoords, string indent) //TODO: either respect or remove "indent"
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
        
        public string StrSquare(SquareState square)
        {
            switch (square.PieceType)
            {
                case State.PieceType.Standard | State.PieceType.Black:
                    return "[\u25A1]";
                case State.PieceType.Standard:
                    return "(O)";
                case State.PieceType.King | State.PieceType.Black:
                    return "[K]";
                case State.PieceType.King:
                    return "(K)";
                case State.PieceType.Empty:
                    return "   ";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
