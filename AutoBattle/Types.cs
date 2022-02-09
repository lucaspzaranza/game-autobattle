using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutoBattle
{
    public class Types
    {
        // Commenting this struct 'cause in my implementation there are not specific character logics.
        //public struct CharacterClassSpecific
        //{
        //    CharacterClass CharacterClass;
        //    float hpModifier;
        //    float ClassDamage;
        //    CharacterSkills[] skills;
        //}

        public struct GridBox
        {
            public int xIndex;
            public int yIndex;
            public bool ocupied;
            public int Index;
            public int PlayerIndex;
            public CharacterClass CharacterClassType;

            public GridBox(int x, int y, bool ocupied, int index)
            {
                xIndex = x;
                yIndex = y;
                this.ocupied = ocupied;
                this.Index = index;
                PlayerIndex = -1;
                CharacterClassType = 0;
            }

            public void ResetGridBox()
            {
                this.ocupied = false;
                PlayerIndex = -1;
                CharacterClassType = 0;
            }
        }

        // Commenting this struct 'cause in my implementation there are not specific character logics.
        //public struct CharacterSkills
        //{
        //    string Name;
        //    float damage;
        //    float damageMultiplier;
        //}

        // I created this struct, simulatint the Unity Vector2 class to grab the x and y coordinates..
        public struct Vector2
        {
            public Vector2(int newX, int newY)
            {
                x = newX;
                y = newY;
            }

            public int x;
            public int y;

            // If I need to write it on the screen, we'll use this.
            public override string ToString()
            {
                return $"[{x}, {y}]";
            }
        }

        public enum CharacterClass : uint
        {
            None = 0,
            Paladin = 1,
            Warrior = 2,
            Cleric = 3,
            Archer = 4
        }

        /// <summary>
        /// Created to select between one of the axis to console read/write operations.
        /// </summary>
        public enum Axis
        {
            XAxis = 0,
            YAxis = 1
        }

        /// <summary>
        /// I've created it to console printing purposes.
        /// We select a direction and write it on the console with the appropriate name.
        /// </summary>
        public enum Direction
        {
            [Display(Name = "up")]
            Up = 1,
            [Display(Name = "down")]
            Down = 2,
            [Display(Name = "upper left")]
            UpperLeft = 3,
            [Display(Name = "left")]
            Left = 4,
            [Display(Name = "lower left")]
            LowerLeft = 5,
            [Display(Name = "upper right")]
            UpperRight = 6,
            [Display(Name = "right")]
            Right = 7,
            [Display(Name = "lower right")]
            LowerRight = 8
        }
    }
}
