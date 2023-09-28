using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LightWeightAppLauncher
{
    internal class InputManager
    {
        
        /// <summary>
        /// Keys currently on cooldown and when cooldown expires
        /// </summary>
        public static Dictionary<string, DateTime> KeysOnCooldown = new Dictionary<string, DateTime>();

        /// <summary>
        /// Removes keys if they have expire
        /// </summary>
        public static void UpdateTimestamps()
        {
            List<string> KeysToRemove = new List<string>();
            foreach (KeyValuePair<string, DateTime> keyandTime in KeysOnCooldown)
            {
                if (DateTime.Now > keyandTime.Value)
                {
                    KeysToRemove.Add(keyandTime.Key);
                }
            }
            foreach (string keytoremove in KeysToRemove)
            {
                KeysOnCooldown.Remove(keytoremove);
            }
        }

        /// <summary>
        /// Gets the current key pressed (first if multiple), returns null if no found or on cooldown, returns CLOSEPROGRAM if user wants to exit
        /// </summary>
        /// <returns></returns>
        public static string? GetPressedKeyOrNull()
        {
            UpdateTimestamps();
            List<Key> keys = InputManager.GetPressedKeys();

            if (keys.Count != 0)
            {
                // If the key is on cooldown
                if (InputManager.KeysOnCooldown.Keys.Contains(keys[0].ToString())) { return null; }

                // If esc close program
                if (keys[0] == Key.Escape) { return "CLOSEPROGRAM"; }

                // add key to cooldown
                InputManager.KeysOnCooldown.Add(keys[0].ToString(), DateTime.Now.Add(TimeSpan.FromSeconds(1)));

                return keys[0].ToString();
            }
            return null;
        }

        /// <summary>
        /// Checks if any key is pressed and puts that into a list
        /// </summary>
        /// <returns></returns>
        public static List<Key> GetPressedKeys()
        {
            var allPossibleKeys = Enum.GetValues(typeof(Key));
            List<Key> results = new List<Key>();
            foreach (var currentKey in allPossibleKeys)
            {
                Key key = (Key)currentKey;
                if (key != Key.None)
                {
                    // Use Dispatcher to invoke on the UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Keyboard.IsKeyDown((Key)currentKey))
                        {
                            results.Add((Key)currentKey);
                        }
                    });
                }
            }
            return results;
        }
    }
}
