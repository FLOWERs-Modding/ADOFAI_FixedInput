using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace FixedInput
{
    
    public class InputPatch
    {
        private static int[] StrangeKeys = {
            160, //LeftShift
            161, //RightShift
            25, //RightControl
            21 //RightAlt
        };

        private static bool[] pressedKey2 =
        {
            false,
            false,
            false,
            false
        };
        

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);
        

        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        public class CountValidKeysPressed
        {
            public static bool Prefix(ref int __result)
            {
                
                if (!Main.isEnabled) return true;
                if (scrController.instance.isCLS) return true;
                if (Input.GetKeyDown(KeyCode.BackQuote)) return true;

                var num = 0;
                
                //마우스 입력 
                for (var j = 323; j <= 329; j++)
                {
                    if (num > 4)
                    {
                        __result = 4;
                        return false;
                    } 
                    
                    if (Input.GetKeyDown((KeyCode)j)) num++;
                }
                
                //키보드 입력 
                for (var k = 0; k < 128; k++)
                {
                    if(Main.KeyKeySetting.useKeyLimit)
                        if (!Main.KeyKeySetting.registerKeys.Contains(k) && scrController.instance.gameworld) continue;
                    
                    if (num > 4)
                    {
                        __result = 4;
                        return false;
                    }
                    
                    if (Input.GetKeyDown((KeyCode)k)) num++;
                }
                
                //특수키 입력
                for (var i = 256; i < 320; i++)
                {
                    if(Main.KeyKeySetting.useKeyLimit)
                        if (!Main.KeyKeySetting.registerKeys.Contains(i) && scrController.instance.gameworld) continue;
                    
                    if (num > 4)
                    {
                        __result = 4;
                        return false;
                    }
                    
                    if((int)KeyCode.LeftShift==i||(int)KeyCode.RightShift==i) continue;
                    if (Input.GetKeyDown((KeyCode)i)) num++;
                }

                //이상한 키들 ( 예 : 쉬프트, 알트 등등 )
                if (!Application.isFocused) return true;
                for(var n = 0; n < 4; n++)
                {
                    if(Main.KeyKeySetting.useKeyLimit)
                        if (!Main.KeyKeySetting.registerKeys.Contains(n) && scrController.instance.gameworld) continue;
                    
                    if (num > 4)
                    {
                        __result = 4;
                        return false;
                    }
                    
                    if ((GetAsyncKeyState(StrangeKeys[n]) & 0x8000) > 0)
                    {
                        if (!pressedKey2[n])
                        {
                            num++;
                            pressedKey2[n] = true;

                        }
                    }
                    else
                    {
                        pressedKey2[n] = false;
                    }
                }
                
                __result = Mathf.Min(4, num);
                return false;

            }
            
            
        }
        
    }
}