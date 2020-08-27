﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConsoleApp
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
        public bool GetInput(bool blackTurn)
        {
            while (true)
            {
                string str = RequestInput(blackTurn);

                if (str == "EXIT") return false;

                bool success = true;
                Direction direction = Direction.Northeast;
                int h = 0, v = 0;

                if (str.Length == 5 && str[2] == ' ')
                {
                    success = false;
                }
                else
                {
                    h = str[0] - 'A' + 1;
                    v = str[0] - '1' + 1;

                    switch (str.Substring(3))
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

                MoveRequested(this, e);
            }

        }

        private string RequestInput(bool blackTurn)
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
                    Console.WriteLine("Please type the desired move by indicating the location of the piece followed by the direction to move it.");
                    Console.WriteLine("For example: A1 SE");
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

            Console.WriteLine("NW   A  B  C  D  E  F  G  H   NE");
            Console.WriteLine("   ┌────────────────────────┐   ");
            Console.Write(StrState(state, true, ""));
            Console.WriteLine("   └────────────────────────┘   ");
            Console.WriteLine("SW                            SE");
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
                    return "(Ö)";
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
