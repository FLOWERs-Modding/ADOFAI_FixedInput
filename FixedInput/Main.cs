using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;

namespace FixedInput
{
    public class Main
    {
        public static bool isEnabled, isRegistering = false;
        private static Harmony harmony;
        private static Dictionary<int, bool> maskedKey = new Dictionary<int, bool>();
        public static KeySetting KeyKeySetting;
        public static Dictionary<int,string> StrangeKeys = new Dictionary<int, string>
        {
            {160,"LeftShift"}, //LeftShift
            {161,"RightShift"}, //RightShift
            {25,"RightControl"}, //RightControl
            {21,"RightAlt"} //RightAlt
        };
        
        public static void Setup(ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            KeyKeySetting = new KeySetting();
            KeyKeySetting = ModSettings.Load<KeySetting>(modEntry);
            
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;
            
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

        private static void OnHideGUI(ModEntry modEntry)
        {
            isRegistering = false;
        }

        private static void OnGUI(ModEntry modEntry)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) isRegistering = false;
            
            KeyKeySetting.useKeyLimit = GUILayout.Toggle(KeyKeySetting.useKeyLimit, RDString.language==SystemLanguage.Korean? "등록된 키만 사용하게 하기.":"Allow only registered keys to be used.");
            
            if (KeyKeySetting.useKeyLimit)
            {
                var str = "";
                foreach (var k in KeyKeySetting.registerKeys) str += (StrangeKeys.ContainsKey(k)? StrangeKeys[k]:((KeyCode)k).ToString())+", ";
                
                GUILayout.Label("     "+str);
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button(RDString.language == SystemLanguage.Korean ? (!isRegistering? "키 등록하기":"등록 완료") : (!isRegistering? "Registering keys":"Stop Registering")))
                {
                    isRegistering = !isRegistering;
                }

                if (isRegistering)
                {
                    foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                    {
                        if(k==KeyCode.Mouse0||k==KeyCode.Mouse1||k==KeyCode.Escape||
                           k==KeyCode.LeftShift||k==KeyCode.RightShift) continue;
                        if (!maskedKey.ContainsKey((int)k)) maskedKey[(int)k] = false;
                        
                        if (Input.GetKeyDown(k))
                        {
                            if (!maskedKey[(int)k])
                            {
                                maskedKey[(int) k] = true;
                                if (!KeyKeySetting.registerKeys.Contains((int) k))
                                    KeyKeySetting.registerKeys.Add((int) k);
                                else
                                    KeyKeySetting.registerKeys.Remove((int) k);
                            }
                        }
                        else
                        {
                            maskedKey[(int) k] = false;
                        }
                    }

                    foreach (var i in StrangeKeys.Keys)
                    {
                        if (!maskedKey.ContainsKey(i)) maskedKey[i] = false;
                        if ((InputPatch.GetAsyncKeyState(i) & 0x8000) > 0)
                        {
                            if (!maskedKey[i])
                            {
                                maskedKey[i] = true;
                                if (!KeyKeySetting.registerKeys.Contains(i))
                                    KeyKeySetting.registerKeys.Add(i);
                                else
                                    KeyKeySetting.registerKeys.Remove(i);
                            }
                        }
                        else
                        {
                            maskedKey[i] = false;
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                
                
            }
        }

        public static void OnSaveGUI(ModEntry modEntry)
        {
            KeyKeySetting.Save(modEntry);
        }

    }
}