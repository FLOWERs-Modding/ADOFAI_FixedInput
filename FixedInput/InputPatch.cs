﻿using System;
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


        public static bool playStateIsWon = false;
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
                playStateIsWon = false;
                AsyncInput.keyQueue.Clear();
                InputManager.ClearMask();
            }
        }
        
        [HarmonyPatch(typeof(scrController), "PlayerControl_Enter")]
        private static class FirstHitTimingPatch
        {
            public static void Postfix()
            {
                AsyncInput.keyQueue.Clear();
                playStateIsWon = false;

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

        [HarmonyPatch(typeof(scrController), "OnLandOnPortal")]
        private static class PlanetEndPatch
        {
            public static void Prefix()
            {
                if (scrController.isGameWorld)
                    playStateIsWon = true;
            }
        }

        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        private static class InputDetectUpdatePatch
        {
            public static void Postfix()
            {
                if (RDC.auto||AudioListener.pause||scrController.instance.isCLS || !scrController.isGameWorld) return;
                if (playStateIsWon) return;
                //if (scrController.instance.currFloor?.prevfloor?.holdLength > -1 && scrController.instance.holding) return;
                if (scrController.instance.currFloor?.holdLength > -1)
                {
                    AsyncInput.keyQueue.Clear();
                    return;
                }
                //if (scrController.instance.currFloor?.nextfloor?.holdLength > -1) return;
                
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

                    
                    var multipress = false;
                    if (AsyncInput.keyQueue.Count == 1) scrController.instance.consecMultipressCounter = 0;
                    while (AsyncInput.keyQueue.Any())
                    {
                        var refAngle = AsyncInput.keyQueue.Dequeue();
                        var rad = (scrMisc.TimeToAngleInRad(refAngle,
                            (float) ((double) scrConductor.instance.bpm * scrController.instance.speed),
                            scrConductor.instance.song.pitch));
                        
                       // Main.logger.Log("----------------");
                        scrController.instance.chosenplanet.angle = scrController.instance.chosenplanet.targetExitAngle - (rad * (scrController.instance.isCW? -1:1));
                        if (multipress)
                        {
                            scrController.instance.chosenplanet.angle =
                                (scrController.instance.currFloor.exitangle -
                                 (rad * (scrController.instance.isCW ? -1 : 1)));
                           // Main.logger.Log("multipress fix: "+scrController.instance.chosenplanet.angle+"\nexitAngle: "+scrController.instance.currFloor.exitangle+"\nrefAngle: "+refAngle+"\nrad: "+rad);
                        }
         
                        scrController.instance.Hit();
                        multipress = true;

                    }
                }
            }
        }


        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        public class DisableOriginalInputPatch
        {
            public static bool Prefix(ref int __result)
            {
                if (scrController.isGameWorld && (scrController.States) scrController.instance.GetState() !=
                    scrController.States.PlayerControl) return true;
                if (RDC.auto||AudioListener.pause||scrController.instance.isCLS || !scrController.isGameWorld) return true;
                //if (scrController.instance.currFloor?.holdLength > -1) return true;
                if (scrController.instance.currFloor?.nextfloor.holdLength > -1) return true;
                __result = 0;
                return false;
            }
        }
        

       
        
    }
}