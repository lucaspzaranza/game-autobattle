using System;
using static AutoBattle.Character;
using static AutoBattle.Grid;
using System.Collections.Generic;
using System.Linq;
using static AutoBattle.Types;

namespace AutoBattle
{
    class Program
    {
        private const int xMaxRows = 10;
        private const int yMaxColumns = 10;
        private const string errorInputMessage = "Invalid value. Please try again.";

        public static int playerInitIndex;
        public static int enemyInitIndex;

        static void Main(string[] args)
        {
            int xSize = 0, ySize = 0;
           
            CharacterClass playerCharacterClass;
            GridBox PlayerCurrentLocation;
            GridBox EnemyCurrentLocation;
            Character PlayerCharacter;
            Character EnemyCharacter;
            List<Character> AllPlayers = new List<Character>();
            int currentTurn = 0;

            // Reorganizing the game setup. Before initialize the grid we read the user input.
            Grid grid;
            Setup();

            void Setup()
            {
                // Before we initialize the grid, we read the axis values from the user input.
                GetBattlefieldSize();

                grid = new Grid(xSize, ySize);
                int numberOfPossibleTiles = grid.grids.Count;
                GetPlayerChoice();
            }

            // Function to check if the axis input value is right. 
            // Must be bigger than one and less or equals than the maximum (10).
            bool AxisSizeIsValid(Axis currentAxis)
            {
                bool result = true;
                if (currentAxis == Axis.XAxis)
                    result = xSize > 1 && xSize <= xMaxRows;
                else if (currentAxis == Axis.YAxis)
                    result = ySize > 1 && ySize <= yMaxColumns;

                return result;
            }

            // Function created to customize the battlefield size.
            void GetBattlefieldSize()
            {
                ReadNumberOfRows();
                ReadNumberOfColumns();
            }

            // Reading the X Axis Size
            void ReadNumberOfRows()
            {
                Console.Write("Choose the battlefield's number of ROWS: ");
                string xChoice = Console.ReadLine();
                xSize = Utils.IntFromConsoleInput(xChoice);

                if(!AxisSizeIsValid(Axis.XAxis))
                {
                    Console.WriteLine(errorInputMessage);
                    ReadNumberOfRows();
                }
            }

            // Reading the Y Axis Size
            void ReadNumberOfColumns()
            {
                Console.Write("Now choose the battlefield's number of COLUMNS: ");

                string yChoice = Console.ReadLine();
                ySize = Utils.IntFromConsoleInput(yChoice);

                if (!AxisSizeIsValid(Axis.YAxis))
                {
                    Console.WriteLine(errorInputMessage);
                    ReadNumberOfColumns();
                }
            }

            void GetPlayerChoice()
            {
                // Asks for the player to choose between for possible classes via console.
                Console.WriteLine("Choose Between One of this Classes:\n");
                Console.WriteLine("[1] Paladin, [2] Warrior, [3] Cleric, [4] Archer");
                // Store the player choice in a variable
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreatePlayerCharacter(Int32.Parse(choice));
                        break;
                    case "2":
                        CreatePlayerCharacter(Int32.Parse(choice));
                        break;
                    case "3":
                        CreatePlayerCharacter(Int32.Parse(choice));
                        break;
                    case "4":
                        CreatePlayerCharacter(Int32.Parse(choice));
                        break;
                    default:
                        GetPlayerChoice();
                        break;
                }
            }

            void CreatePlayerCharacter(int classIndex)
            {
                playerInitIndex = new Random().Next(0, 2);

                playerCharacterClass = (CharacterClass)classIndex;
                Console.WriteLine($"Player Class Choice: {playerCharacterClass}");

                // Setting the Player Character default values
                PlayerCharacter = new Character(playerCharacterClass);
                PlayerCharacter.Name = $"{playerCharacterClass} Player";
                PlayerCharacter.Health = 100;
                PlayerCharacter.BaseDamage = 20;
                PlayerCharacter.PlayerIndex = playerInitIndex;
                
                CreateEnemyCharacter();
            }

            void CreateEnemyCharacter()
            {
                // The complementary index to the enemy. 
                // If the player = 0, then the enemy will be 1, and vice-versa.
                enemyInitIndex = (playerInitIndex + 1) % 2;

                //randomly choose the enemy class and set up vital variables
                var rand = new Random();
                int randomInteger = rand.Next(1, 4);
                CharacterClass enemyClass = (CharacterClass)randomInteger;

                Console.WriteLine($"Enemy Class Choice: {enemyClass}");
                Utils.PrintPressAnyKeyToContinue();

                EnemyCharacter = new Character(enemyClass);
                EnemyCharacter.Name = $"{enemyClass} Enemy";
                EnemyCharacter.Health = 100;
                EnemyCharacter.BaseDamage = 20;
                EnemyCharacter.PlayerIndex = enemyInitIndex;

                StartGame();
            }

            void StartGame()
            {
                // Populates the character variables and targets
                EnemyCharacter.Target = PlayerCharacter;
                PlayerCharacter.Target = EnemyCharacter;
                AllPlayers.Add(PlayerCharacter);
                AllPlayers.Add(EnemyCharacter);
                AlocatePlayers();
                StartTurn();
            }

            void StartTurn(){

                if (currentTurn == 0)
                    AllPlayers.Sort();

                foreach (Character character in AllPlayers)
                {
                    // If some character has health <= 0, then he's dead, and it's not necessary to start one more turn anymore.
                    if (character.Health <= 0) 
                        break;

                    character.StartTurn(grid);
                }

                currentTurn++;
                HandleTurn();
            }

            void HandleTurn()
            {
                if(PlayerCharacter.Health <= 0)
                {
                    Console.Write(Environment.NewLine + Environment.NewLine);
                    Console.WriteLine("Game over! Enemy has won the game.");

                    Utils.PrintPressAnyKeyToContinue();
                    return;
                } 
                else if (EnemyCharacter.Health <= 0)
                {
                    Console.Write(Environment.NewLine + Environment.NewLine);
                    Console.WriteLine("Game over! Player has won the game.");
                    Console.Write(Environment.NewLine + Environment.NewLine);

                    Utils.PrintPressAnyKeyToContinue();

                    return;
                }
                else
                {
                    // For design purposes...
                    if(grid.xLength <= 2)
                        Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);

                    Console.WriteLine("End of the turn.\nClick on any key to start the next turn...");
                    Console.ReadKey();
                    StartTurn();
                }
            }

            int GetRandomInt(int min, int max)
            {
                var rand = new Random();
                int index = rand.Next(min, max);
                return index;
            }

            void AlocatePlayers()
            {
                AlocatePlayerCharacter();
            }

            void AlocatePlayerCharacter()
            {
                int random = GetRandomInt(0, grid.GridCount);
                GridBox RandomLocation = grid.grids.ElementAt(random);

                if (!RandomLocation.ocupied)
                {
                    // Removing the another PlayerCurrentLocation, it's unecessary since we already have the global declaration of it.
                    PlayerCurrentLocation = RandomLocation;
                    PlayerCurrentLocation.ocupied = true;
                    PlayerCurrentLocation.PlayerIndex = PlayerCharacter.PlayerIndex;
                    PlayerCurrentLocation.CharacterClassType = PlayerCharacter.CharacterClass;

                    grid.grids[random] = PlayerCurrentLocation;
                    PlayerCharacter.currentBox = grid.grids[random];
                    AlocateEnemyCharacter();
                } 
                else
                    AlocatePlayerCharacter();
            }

            void AlocateEnemyCharacter()
            {
                // Changing the random position calculus
                int randomIndex = GetRandomInt(0, grid.GridCount);
                GridBox RandomLocation = grid.grids.ElementAt(randomIndex);

                if (!RandomLocation.ocupied)
                {
                    EnemyCurrentLocation = RandomLocation;
                    EnemyCurrentLocation.ocupied = true;
                    EnemyCurrentLocation.PlayerIndex = EnemyCharacter.PlayerIndex;
                    EnemyCurrentLocation.CharacterClassType = EnemyCharacter.CharacterClass;

                    grid.grids[randomIndex] = EnemyCurrentLocation;
                    EnemyCharacter.currentBox = grid.grids[randomIndex];

                    // Customizing the battlefield matrix drawing
                    grid.DrawBattlefield(grid.xLength, grid.yLength);
                    Console.WriteLine("Let the game begin!");
                    Console.ReadKey();
                }
                else
                    AlocateEnemyCharacter();
            }
        }
    }
}
