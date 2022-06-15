using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using MonsterLove.StateMachine;
using UnityEngine;

namespace FixedInput
{
    
    public class InputPatch
    {
        

        [HarmonyPatch(typeof(scrController), "OnApplicationQuit")]
        public static class OnExitThreadStopPatch
        {
            public static void Postfix()
            {
                if(Main.KeyKeySetting.useAsync) AsyncInput.Stop();
            }
        }

        [HarmonyPatch(typeof(scrController), "Awake")]
        private static class ResetKeyInputPatch
        {
            public static void Postfix()
            {
                Clear();
            }
        }
        
        [HarmonyPatch(typeof(scrController), "PlayerControl_Enter")]
        private static class FirstHitTimingPatch
        {
            public static void Postfix()
            {
                Clear();

                if (Main.KeyKeySetting.useAsync && scrController.isGameWorld)
                {
                    var f = scrController.instance.currFloor;
                    if (f == null) return;
                    if (f.nextfloor == null) return;
                    AsyncInput.hitTimeFloor = scrConductor.instance.dspTimeSongPosZero + f.nextfloor.entryTimePitchAdj +
                                              (scrConductor.currentPreset.inputOffset / 1000.0);
                }
            }
        }

        [HarmonyPatch(typeof(scrPlanet), "MoveToNextFloor")]
        private static class SetAfterHitTimingPatch
        {
            public static void Prefix(scrFloor floor)
            {
                if (Main.KeyKeySetting.useAsync && scrController.isGameWorld && floor.nextfloor != null)
                    AsyncInput.hitTimeFloor = scrConductor.instance.dspTimeSongPosZero +
                                              floor.nextfloor.entryTimePitchAdj +
                                              (scrConductor.currentPreset.inputOffset / 1000.0);
            }
        }

        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        private static class InputDetectUpdatePatch
        {
            public static void Postfix()
            {
                if (RDC.auto||AudioListener.pause||scrController.instance.isCLS || !scrController.isGameWorld) return;
                
                if (!Main.KeyKeySetting.useAsync)
                {
                    var kc = InputManager.GetKeyCount();
                    if (kc > 0)
                    {
                        if (kc == 1) scrController.instance.consecMultipressCounter = 0;
                        for (var n = 0; n < kc; n++)
                            scrController.instance.keyTimes.Add(Time.timeAsDouble);
                    }

                }
                else
                {
                    var multipress = AsyncInput.keyQueue.Count > 1;
                    if (AsyncInput.keyQueue.Count == 1) scrController.instance.consecMultipressCounter = 0;
                    while (AsyncInput.keyQueue.Any())
                    {
                        var refAngle = AsyncInput.keyQueue.Dequeue();
                        var rad = (scrMisc.TimeToAngleInRad(refAngle,
                            (float) ((double) scrConductor.instance.bpm * scrController.instance.speed),
                            scrConductor.instance.song.pitch));
                        
                        
                        scrController.instance.chosenplanet.angle = scrController.instance.chosenplanet.targetExitAngle - (rad * (scrController.instance.isCW? -1:1));
                        if (multipress)
                        {
                            scrController.instance.chosenplanet.angle =
                                (scrController.instance.currFloor.exitangle -
                                 (rad * (scrController.instance.isCW ? -1 : 1)));
                        }
                        scrController.instance.Hit();

                    }
                }
            }
        }


        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        public class DisableOriginalInputPatch
        {
            public static bool Prefix(ref int __result)
            {
                if (RDC.auto||AudioListener.pause||scrController.instance.isCLS || !scrController.isGameWorld) return true;
                __result = 0;
                return false;

            }
        }
        
        private static void Clear()
        {
            AsyncInput.keyQueue.Clear();
            InputManager.ClearMask();
        }

       
        
    }
}