using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static AutoBattle.Types;

namespace AutoBattle
{
    public class Grid
    {
        public List<GridBox> grids = new List<GridBox>();
        public int xLength; // Number of rows
        public int yLength; // Number of columns
        public int GridCount; // Array Length

        // Store the values to be printed on the HUD.
        private int playerLifeHUD;
        private int enemyLifeHUD;

        public Grid(int Lines, int Columns)
        {
            xLength = Lines;
            yLength = Columns;

            playerLifeHUD = Character.defaultInitialHealth;
            enemyLifeHUD = Character.defaultInitialHealth;

            GridCount = xLength * yLength;

            Character.OnCharacterLifeChanged += HandleOnCharacterLifeChanged;
            Character.OnCharacterDeath += HandleOnCharacterDeath;

            Console.WriteLine("The battle field has been created\n");
            for (int i = 0; i < Lines; i++)
            {
                for(int j = 0; j < Columns; j++)
                {
                    // I changed the i and j parameter location to grow on the left-right direction first
                    GridBox newBox = new GridBox(i, j, false, (Columns * i + j));
                    grids.Add(newBox);
                }
            }

            DrawBattlefield(Lines, Columns, false, false);
        }

        // When the grid will be destroyed, we remove the listener from this event.
        ~Grid()
        {
            Character.OnCharacterLifeChanged -= HandleOnCharacterLifeChanged;
            Character.OnCharacterDeath -= HandleOnCharacterDeath;
        }        

        // Prints the matrix that indicates the tiles of the battlefield.
        // Added two default parameters to clear console and draw the HUD.
        public void DrawBattlefield(int Lines, int Columns, bool clearConsole = true, bool drawHUD = true)
        {
            // Adding some functionalities to show a better feedback to the players.
            if(clearConsole)
                Console.Clear();

            if(drawHUD)
                DrawHUD();
            
            for (int i = 0; i < Lines; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    // We must find the right index based on the i and j values from the for loops
                    int gridIndex = GetGridIndex(i, j);
                    if (grids[gridIndex].ocupied)
                    {
                        // Write the grid with a player character inside of it. Ex: [A] if it's an archer, [P] if paladin...
                        Console.Write("[");
                        // Changing the color. Player will be green, enemy will be red.
                        if(grids[gridIndex].PlayerIndex == Program.playerIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(GetCharacterClassChar(grids[gridIndex].CharacterClassType));
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if(grids[gridIndex].PlayerIndex == Program.enemyIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(GetCharacterClassChar(grids[gridIndex].CharacterClassType));
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        Console.Write("]\t");
                    }
                    else // Showing an empty grid.
                        Console.Write($"[ ]\t");
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
            Console.Write(Environment.NewLine + Environment.NewLine);
        }

        // For Debug purposes only. It shows each grid element with its xIndex and yIndex.
        public void DrawBattlefieldDebug(int Lines, int Columns)
        {
            Console.Clear();
            for (int i = 0; i < Lines; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    int gridIndex = GetGridIndex(i, j);
                    Console.Write("[");
                    Console.Write($"{grids[gridIndex].xIndex}, {grids[gridIndex].yIndex}");
                    Console.Write("]\t");
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
            Console.Write(Environment.NewLine + Environment.NewLine);
        }

        /// <summary>
        /// Function to draw the game main Heads Up display.
        /// </summary>
        public void DrawHUD()
        {
            // This is just an algorithm to centralize the Autobattle title with the battefield.
            // I figured it out doing some tests.
            int HUDStartIndex = (yLength <= 2) ? 0 : (xLength + (3 * ((yLength - 1) - 3)));

            Console.CursorLeft = (HUDStartIndex >= 0)? HUDStartIndex : 0;
            Console.Write("::: AUTOBATTLE :::");

            HUDStartIndex = (yLength * 10);

            if (yLength == 2) // Specific case
                HUDStartIndex = 30;

            // HUD Title
            Console.CursorLeft = HUDStartIndex + 2;
            Console.CursorTop = 0;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(":::: HUD ::::");

            // Players Status
            if (xLength < 3)
                HUDStartIndex = 30;
            Console.CursorLeft = HUDStartIndex - 5;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Player [{playerLifeHUD}]");
            Console.CursorLeft = HUDStartIndex + 7;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" | ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Enemy [{enemyLifeHUD}]");
            Console.ForegroundColor = ConsoleColor.White;

            // Print a lifebar to each player
            PrintLifebar(playerLifeHUD, HUDStartIndex - 5, ConsoleColor.DarkGreen);
            PrintLifebar(enemyLifeHUD, HUDStartIndex + 9, ConsoleColor.DarkRed);

            Console.Write(Environment.NewLine + Environment.NewLine);
            HUDStartIndex -= 4;
            Console.CursorLeft = HUDStartIndex;
            Console.WriteLine("::: Character Classes :::\n");

            HUDStartIndex += 7;
            Console.CursorLeft = HUDStartIndex;
            Console.WriteLine("P: Paladin");
            Console.CursorLeft = HUDStartIndex;
            Console.WriteLine("W: Warrior");
            Console.CursorLeft = HUDStartIndex;
            Console.WriteLine("C: Cleric");
            Console.CursorLeft = HUDStartIndex;
            Console.WriteLine("A: Archer");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            Console.Write(Environment.NewLine);
        }

        /// <summary>
        /// Prints the lifebar into the HUD.
        /// </summary>
        /// <param name="life">The life value.</param>
        /// <param name="xCoordinate">The X coordinate start position to calculate the spacing.</param>
        /// <param name="lifebarColor">The lifebar color.</param>
        private void PrintLifebar(int life, int xCoordinate, ConsoleColor lifebarColor)
        {
            // Player lifebar

            int barPercentage = life / 10; // The maxium will be ten characters.
            Console.CursorLeft = xCoordinate;
            Console.Write('[');
            Console.BackgroundColor = lifebarColor;

            // If the barPercentage is between [1,10], then prints the lifebar right amount.
            if(barPercentage > 0)
            {
                // Prints the life amount based on the player life. If 100, then 100 / 10 = 10, so we'll paint 10 green whitespaces.
                // If, for example, the life equals 43, then we have: 43 / 10 = 4, so we'll paint 4 green whitespaces.
                for (int i = 0; i < barPercentage; i++)
                {
                    Console.Write(' ');
                }
            }
            // If barPercentage equals zero and has some life yet, prints only one lifebar character.
            else if (barPercentage == 0 && life > 0) 
            {
                Console.BackgroundColor = Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = lifebarColor;
                Console.Write('|');
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            // Filling the empty space of the lifebar. If zero, it'll paint nothing. If, for example, the barPercentage is 40%,
            // i.e, 4 bars, the empty area will be 6.
            Console.BackgroundColor = ConsoleColor.Black;
            for (int i = barPercentage; i < 10; i++)
            {
                Console.Write(' ');
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(']');
        }

        /// <summary>
        /// Get Character based on character class type.
        /// </summary>
        /// <param name="currentClass">Current character class type.</param>
        /// <returns>The First letter from its type. If not found, will return an 'X'.</returns>
        public char GetCharacterClassChar(CharacterClass currentClass)
        {
            char result = 'X';

            switch (currentClass)
            {
                case CharacterClass.Paladin:
                    result = 'P';
                    break;
                case CharacterClass.Warrior:
                    result = 'W';
                    break;
                case CharacterClass.Cleric:
                    result = 'C';
                    break;
                case CharacterClass.Archer:
                    result = 'A';
                    break;
                default:
                    result = 'X';
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns the grid array index based on the grid matrix coordinate.
        /// </summary>
        /// <param name="xIndex">The matrix x coordinate.</param>
        /// <param name="yIndex">The matrix y coordinate.</param>
        /// <returns></returns>
        public int GetGridIndex(int xIndex, int yIndex)
        {
            // Logic explanation:
            // Since our array is being drawn first on the column, we need to use it as the 
            // main variable and use its axis to find our grid index. For example, let's use
            // a 2x3 grid and get the [1,1] coordinate: 

            //    0   1   2
            // 0 [ ] [ ] [ ]
            // 1 [ ] [X] [ ]

            //                                                                 0   1   2   3   4   5
            // If we transform it into an one-dimensional array we would get: [ ] [ ] [ ] [ ] [X] [ ].

            // First we use the x coordinate (1) to calculate the row multiplier by the number of elements in a column (3):
            // 1 * 3 = 3.
            // It will find the matrix row index inside the array. Then we use the y coordinate (1) to iterate through that row
            // and get the correct position based on the row coordinate. We'll use the addition operation:
            // 3 + 1 = 4.
            // Voilà.

            return (xIndex * yLength) + yIndex;
        }

        /// <summary>
        /// When a Character will be attacked, we'll refresh the screen.
        /// </summary>
        /// <param name="character">The character who has its life value changed.</param>
        /// <param name="drawBattlefield">Flag to pass if you want to override the battlefield refreshing.</param>
        public void HandleOnCharacterLifeChanged(Character character, bool drawBattlefield = true)
        {
            if (character.PlayerIndex == Program.playerIndex)
                playerLifeHUD = character.Health;
            else if (character.PlayerIndex == Program.enemyIndex)
                enemyLifeHUD = character.Health;

            if(drawBattlefield)
                DrawBattlefield(xLength, yLength);
        }

        /// <summary>
        /// Updates the grid list with the dead character grid box data.
        /// </summary>
        /// <param name="characterGridBox">The character grid box with its reseted data.</param>
        public void HandleOnCharacterDeath(GridBox characterGridBox)
        {
            grids[characterGridBox.Index] = characterGridBox;
            // Drawing one more time the battlefield to refresh the screen with the dead character no more appearing.
            DrawBattlefield(xLength, yLength);
        }
    }
}
