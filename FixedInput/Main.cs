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
        public static UnityModManager.ModEntry.ModLogger logger;
        public static KeySetting KeyKeySetting;
        public static DateTime AdofaiStartTime;


        public static void Setup(ModEntry modEntry)
        {
            harmony = new Harmony(modEntry.Info.Id);
            KeyKeySetting = new KeySetting();
            KeyKeySetting = ModSettings.Load<KeySetting>(modEntry);
            
            AdofaiStartTime =  DateTime.Now.AddSeconds(Time.realtimeSinceStartupAsDouble*-1);
            //var stateTime = (DateTime.Now.Ticks - a.Ticks) / 10000000.0;
            //Debug.Log(stateTime);
            //Debug.Log(Time.realtimeSinceStartupAsDouble);
            
            if(KeyKeySetting.useKeyLimit) InputManager.keyCodes = KeyKeySetting.registerKeys.ToArray();
            
            modEntry.OnToggle = OnToggle;
            
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;
            logger = modEntry.Logger;
            
            

        }
        
        private static bool OnToggle(ModEntry modEntry, bool value)
        {

            isEnabled = value;
            if (value)
            {
                if(KeyKeySetting.useAsync)
                    AsyncInput.Start();
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
                AsyncInput.Stop();
            }
            return true;
        }

        private static void OnHideGUI(ModEntry modEntry)
        {
            if (isRegistering)
            {
                InputManager.keyCodes = KeyKeySetting.registerKeys.ToArray();
            }
            isRegistering = false;
        }

        private static void OnGUI(ModEntry modEntry)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) isRegistering = false;

            var newV2  = GUILayout.Toggle(KeyKeySetting.useKeyLimit, RDString.language==SystemLanguage.Korean? "등록된 키만 사용하게 하기.":"Allow only registered keys to be used.");
            if (KeyKeySetting.useKeyLimit != newV2)
            {
                InputManager.keyCodes = KeyKeySetting.registerKeys.ToArray();
                KeyKeySetting.useKeyLimit = newV2;
            }
            var newV = GUILayout.Toggle(KeyKeySetting.useAsync, RDString.language==SystemLanguage.Korean? "비동기 입력 사용하기":"Use asynchronous input.");
            if (KeyKeySetting.useAsync != newV)
            {
                KeyKeySetting.useAsync = newV;
                if (newV) AsyncInput.Start();
                else AsyncInput.Stop();
            }
            
            if (KeyKeySetting.useKeyLimit)
            {
                var str = "";
                foreach (var k in KeyKeySetting.registerKeys) str += ((VirtualKeys)k)+", ";
                
                GUILayout.Label("     "+str);
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button(RDString.language == SystemLanguage.Korean ? (!isRegistering? "키 등록하기":"등록 완료") : (!isRegistering? "Registering keys":"Stop Registering")))
                {
                    isRegistering = !isRegistering;
                    if (!isRegistering)
                    {
                        InputManager.keyCodes = KeyKeySetting.registerKeys.ToArray();
                    }
                }

                if (isRegistering)
                {
                    foreach (VirtualKeys k in Enum.GetValues(typeof(VirtualKeys)))
                    {
                        if(k==VirtualKeys.LeftButton||k==VirtualKeys.RightButton||k==VirtualKeys.Escape) continue;
                        if (!maskedKey.ContainsKey((int)k)) maskedKey[(int)k] = false;
                        
                        if ((InputManager.GetAsyncKeyState((int)k) & 0x8000) > 0)
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
                    
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                
            }
        }

        public static void OnSaveGUI(ModEntry modEntry)
        {
            KeyKeySetting.Save(modEntry);
        }

    }
}