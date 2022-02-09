using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static AutoBattle.Types;

namespace AutoBattle
{
    public class Character : IComparable // Adding this interface to sort them
    {
        #region Vars

        // Changing the float type to integer type.
        public int Health;
        public int BaseDamage;

        public GridBox currentBox;

        // This event will be fired when the character will be attacked
        // Whoever listens to this will perform some action.
        public delegate void CharacterAttacked(Character target);
        public static CharacterAttacked OnCharacterAttacked;

        public delegate void CharacterDeath(GridBox characterBox);
        public static CharacterDeath OnCharacterDeath;

        #endregion

        #region Props

        public string Name { get; set; }
        public int PlayerIndex { get; set; } // changing this member to be a prop.
        public float DamageMultiplier { get; set; }
        public Character Target { get; set; }
        public CharacterClass CharacterClass { get; set; }
        string CurrentCharacter => PlayerIndex == Program.playerInitIndex ? "Player" : "Enemy";
        /// <summary>
        /// The opposite of the CurrentCharacter prop.
        /// </summary>
        string RemainingCharacter => PlayerIndex == Program.playerInitIndex ? "Enemy" : "Player";

        #endregion

        #region Ctor

        public Character(CharacterClass characterClass)
        {
            CharacterClass = characterClass;
        }

        #endregion

        #region Methods

        // Updates:
        // - Changing the damage taking logic. Now we subtract the health by the amount passed as a parameter.
        // - Changing the return type to void.
        public void TakeDamage(int amount)
        {
            Health -= amount;

            if(Health <= 0)
                Die();
        }

        // Updates:
        // - Reseting the current box values
        // - Firing an event to the character death.
        public void Die()
        {
            currentBox.ResetGridBox();
            OnCharacterDeath?.Invoke(currentBox);
        }

        // I made some changes on the WalkTo method. 
        //public void WalkTo(Grid battlefield, Direction directionToMove)
        public void WalkTo(Grid battlefield)
        {
            // I'll use the predicates dynamically and store them in a variable to use later.
            Predicate<GridBox> predicate = null;

            // First we get the distance from the character to its opponent.
            Vector2 distance = new Vector2(
                currentBox.xIndex - Target.currentBox.xIndex,
                currentBox.yIndex - Target.currentBox.yIndex);
            Direction currentDirection = 0;

            // Based on the distance values we check the 8 directions:
            // [0,0]   [0,1]  [0,2]        
            //       ↖   ↑   ↗                
            // [1,0] ← [1,1] → [1,2]        
            //       ↙   ↓   ↘                
            // [2,0]   [2,1]   [2,2]

            // An offset to get the next or previous row index value.
            // If > 0, then the target is ABOVE the current character, with a  LOWER row value.
            // If < 0, then the target is BELOW the current character, with an UPPER row value.
            int offsetPos = ((distance.x > 0) ? -1 : 1);

            if (distance.y > 0) // Target is on the left, so the y (column) value is different.
            {
                currentDirection = Direction.Left; // default value

                if (distance.x == 0) // Left ← because there is no difference between the x (row) values.
                    predicate = x => x.yIndex == currentBox.yIndex - 1 && x.xIndex == currentBox.xIndex; // Getting the element with column value equals - 1
                else
                {
                    // If distance.x > 0 = ↖ | If distance.x < 0 = ↙
                    predicate = x => x.yIndex == currentBox.yIndex - 1 && x.xIndex == currentBox.xIndex + offsetPos;

                    // Getting the corner movements enum value based on the distance.x;
                    currentDirection = (Direction)((int)Direction.Left + offsetPos);
                }
            }
            else if(distance.y < 0) // Target is on the right, so the y (column) value is changing.
            {
                // The same logic of the previous if statement
                currentDirection = Direction.Right; // default value

                if (distance.x == 0) // Right → with the same row position
                    predicate = x => x.yIndex == currentBox.yIndex + 1 && x.xIndex == currentBox.xIndex;
                else
                {
                    // If distance.x > 0 = ↗ | If distance.x < 0 = ↘
                    predicate = x => x.yIndex == currentBox.yIndex + 1 && x.xIndex == currentBox.xIndex + offsetPos;
                    currentDirection = (Direction)((int)Direction.Right + offsetPos);
                }
            }
            else if(distance.y == 0) // Up or down, same column value
            {
                if(distance.x > 0) // Up ↑
                    predicate = x => x.yIndex == currentBox.yIndex && x.xIndex == currentBox.xIndex - 1;
                else if(distance.x < 0) // Down ↓
                    predicate = x => x.yIndex == currentBox.yIndex && x.xIndex == currentBox.xIndex + 1;

                currentDirection = (distance.x > 0) ? Direction.Up : Direction.Down;
            }

            // Calling the code to set the new gridbox.
            if (battlefield.grids.Exists(predicate))
                SetNetCurrentBox(battlefield, predicate, currentDirection);
        }

        // I got the code on the StartTurn method and paste it here, doing the verification 
        // dynamically with the code below.
        private void SetNetCurrentBox(Grid battlefield, Predicate<GridBox> predicate, Direction newDirection)
        {
            // the currentBox will be empty, so we need to leave it empty.
            currentBox.ocupied = false;
            battlefield.grids[currentBox.Index] = currentBox;

            var newBox = battlefield.grids.Find(predicate);

            Console.WriteLine($"No attacks. Looking for the {RemainingCharacter} position...");
            Console.ReadKey();

            // Updated some stuff on the newBox ocupation logic.
            // Added the CharacterClassType assignment, and the PlayerIndex.
            newBox.ocupied = true;
            newBox.CharacterClassType = currentBox.CharacterClassType;
            newBox.PlayerIndex = currentBox.PlayerIndex;
            battlefield.grids[newBox.Index] = newBox;
            currentBox = newBox;

            // Changing the values to the grid axis lengths
            battlefield.DrawBattlefield(battlefield.xLength, battlefield.yLength);
            Console.WriteLine($"{CurrentCharacter} walked {newDirection.GetDisplayName()}.");
            Utils.PrintPressAnyKeyToContinue();
        }

        public void StartTurn(Grid battlefield)
        {
            // Changes: I put the direction calculation inside the WalkTo method, and the predicate logic as well.

            Console.WriteLine($"\n{CurrentCharacter}'s turn.");

            if (CheckCloseTargets(battlefield)) 
                Attack(Target);
            else
                // If there is no target close enough, calculates in wich direction this character should move to be closer to a possible target
                WalkTo(battlefield);
        }

        // Check in x and y directions if there is any character close enough to be a target.
        // OBS: Since my birthday is 31/08, in August, my challenge is to implement the 8 movement direction.
        bool CheckCloseTargets(Grid battlefield)
        {
            // Changing the close target verification to get the x and y coordinates as the basis.
            // This way we can get the diagonal directions, upper left and right, and lower left and right.
            // The direction verification will be the following:

            // [0,0]   [0,1]   [0,2]        Left: ↖, ← and ↙
            //       ↖   ↑   ↗              Right: ↗, → and ↘
            // [1,0] ← [1,1] → [1,2]        Up: ↑
            //       ↙   ↓   ↘              Down: ↓ 
            // [2,0]   [2,1]   [2,2]        All the 8 directions will be verificated then.

            // Using LINQ to do the "query" I got this logic:
            // Seek for all the matching elements within the list, and look for any of them which fits the 
            // condition ocupied being true.

            bool left = battlefield.grids.Where(
                x => (x.yIndex == currentBox.yIndex - 1 && x.xIndex == currentBox.xIndex) // ←
                ||   (x.yIndex == currentBox.yIndex - 1 && x.xIndex == currentBox.xIndex - 1) // ↖
                ||   (x.yIndex == currentBox.yIndex - 1 && x.xIndex == currentBox.xIndex + 1) // ↙ 
                ).Any(x => x.ocupied);

            bool right = battlefield.grids.Where(
                x => (x.yIndex == currentBox.yIndex + 1 && x.xIndex == currentBox.xIndex) // →
                || (x.yIndex == currentBox.yIndex + 1 && x.xIndex == currentBox.xIndex - 1) // ↗
                || (x.yIndex == currentBox.yIndex + 1 && x.xIndex == currentBox.xIndex + 1) // ↘ 
                ).Any(x => x.ocupied);

            // The up and down verification are simpler since I'm looking for only one direction.
            // The other 6 has been covered on the
            bool up = battlefield.grids.Find(x => x.xIndex == currentBox.xIndex - 1 && x.yIndex == currentBox.yIndex).ocupied;
            bool down = battlefield.grids.Find(x => x.xIndex == currentBox.xIndex + 1 && x.yIndex == currentBox.yIndex).ocupied;

            // Changing the close target logic. We need only one direction to have an enemy.
            // Optimizing the return logic, too.
            return left || right || up || down;
        }

        public void Attack(Character target)
        {
            Console.WriteLine($"{CurrentCharacter} will attack!");
            Console.ReadKey();

            var rand = new Random();
            // Adding the variable damageTaken to get the random Damage amount to take.
            int damageTaken = rand.Next(0, BaseDamage + 1); // Added one to the BaseDamage value be inclusive.
            target.TakeDamage(damageTaken);
            Console.WriteLine($"{CurrentCharacter} is attacking the {RemainingCharacter} and did {damageTaken} damage!\n");
            Console.ReadKey();

            OnCharacterAttacked?.Invoke(target);
        }
        
        // Implementing the interface method "CompareTo"
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Character otherChar = obj as Character;
            return this.PlayerIndex.CompareTo(otherChar.PlayerIndex);
        }

        #endregion
    }
}
