using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace FixedInput
{
    public class Main
    {
        public static bool isEnabled, isRegistering = false;
        private static Harmony harmony;
        private static Dictionary<int, bool> maskedKey = new Dictionary<int, bool>();
        public static Setting keySetting;

        public static void Setup(ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            
            keySetting = new Setting();
            keySetting = ModSettings.Load<Setting>(modEntry);
            
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
        }
        
        private static bool OnToggle(ModEntry modEntry, bool value)
        {

            isEnabled = value;
            if (value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }

        private static void OnGUI(ModEntry modEntry)
        {
            
            keySetting.useKeyLimit = GUILayout.Toggle(keySetting.useKeyLimit, RDString.language==SystemLanguage.Korean? "등록된 키만 사용하게 하기.":"Allow only registered keys to be used.");
            
            if (keySetting.useKeyLimit)
            {
                var str = "";
                foreach (var k in keySetting.registerKeys) str += (KeyCode)k+", ";
                
                GUILayout.Label("     "+str);
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button(RDString.language == SystemLanguage.Korean ? (!isRegistering? "키 등록하기":"등록 완료") : (!isRegistering? "Registering keys":"Stop Registering")))
                {
                    isRegistering = !isRegistering;
                }

                if (isRegistering)
                {
                    foreach (var k in Enum.GetValues(typeof(KeyCode)))
                    {
                        if((KeyCode)k==KeyCode.MouseLeft||(KeyCode)k==KeyCode.MouseRight) continue;
                        if (!maskedKey.ContainsKey((int)k)) maskedKey[(int)k] = false;
                        if ((InputPatch.GetAsyncKeyState((int) k) & 0x8000) > 0)
                        {
                            if (!maskedKey[(int)k])
                            {
                                maskedKey[(int) k] = true;
                                if (!keySetting.registerKeys.Contains((int) k))
                                    keySetting.registerKeys.Add((int) k);
                                else
                                    keySetting.registerKeys.Remove((int) k);
                            }
                        }
                        else
                        {
                            maskedKey[(int) k] = false;
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                
                
            }
        }

        public static void OnSaveGUI(ModEntry modEntry)
        {
            keySetting.Save(modEntry);
        }

    }
}