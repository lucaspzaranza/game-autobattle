using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AutoBattle
{
    /// <summary>
    /// Class to share some utilities used through the solution.
    /// </summary>
    public static class Utils
    {
        private const string pressAnyKeyToContinueMessage = "Press any key to continue...";

        /// <summary>
        /// Reads the input from the console and try to convert it to an integer value.
        /// </summary>
        /// <param name="input">The console input.</param>
        /// <returns>The integer value converted from the string. If invalid, returns zero.</returns>
        public static int IntFromConsoleInput(string input)
        {
            int parsedInt = 0;
            Int32.TryParse(input, out parsedInt);
            return parsedInt;
        }

        /// <summary>
        /// Prints on the console a message and prompts the user action.
        /// </summary>
        public static void PrintPressAnyKeyToContinue()
        {
            Console.WriteLine(pressAnyKeyToContinueMessage);
            Console.ReadKey();
        }

        /// <summary>
        /// Extension method to retrieve the display name into an enumeration.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            string displayName;
            displayName = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();
            if (String.IsNullOrEmpty(displayName))
                displayName = enumValue.ToString();

            return displayName;
        }
    }
}
