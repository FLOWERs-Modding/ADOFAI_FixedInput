using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEngine;

namespace FixedInput
{
    
    public class InputPatch
    {
        private static Dictionary<int, bool> pressedKey = new Dictionary<int, bool>();

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);
        
        
        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        public class CountValidKeysPressed
        {
            public static bool Prefix(ref int __result)
            {
                if (!Main.isEnabled) return true;
                if (!Application.isFocused) return true;
                
                int num = 0;
                foreach (var k in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Main.keySetting.useKeyLimit)
                        if (!Main.keySetting.registerKeys.Contains((int) k)&&scrController.instance.gameworld) continue;
                    

                    if (!pressedKey.ContainsKey((int)k)) pressedKey[(int)k] = false;
                    if ((GetAsyncKeyState((int)k) & 0x8000) > 0)
                    {
                        if (!pressedKey[(int)k])
                        {
                            num++;
                            pressedKey[(int)k] = true;
                        }
                    }
                    else
                    {
                        pressedKey[(int)k] = false;
                    }
                }

                __result = num;
                return false;
            }
        }
        
    }
}