using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FixedInput
{
    public class AsyncInput
    {
        
        public static Thread thread;
        public static Queue<double> keyQueue = new Queue<double>();
        private static double beforeDspTime;
        private static double lastFrame;
        public static double dspTime;
        
        public static void Start()
        {
            if (Main.KeyKeySetting.useAsync) {
                thread = new Thread(Run);
                thread.Start();
            }
        }

        public static void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
            keyQueue.Clear();
        }
        
        
        private static void Run()
        {
            
            long prevTick, currTick;
            prevTick = DateTime.Now.Ticks;
            while (Main.KeyKeySetting.useAsync)
            {
                currTick = DateTime.Now.Ticks;
                if (currTick > prevTick)
                {
                    
                    //var time = 1.0 / (currTick - prevTick);
                    var time = (DateTime.Now.Ticks - Main.AdofaiStartTime.Ticks) / 10000000.0;
                    if (!AudioListener.pause && Application.isFocused)
                    {
                        var num = time - lastFrame;
                        dspTime += num;
                    }

                    lastFrame = time;
                    if (AudioSettings.dspTime != beforeDspTime)
                    {
                        dspTime = AudioSettings.dspTime;
                        beforeDspTime = AudioSettings.dspTime;
                    }
                    prevTick = currTick;
                    UpdateKeyQueue();
                }
            }
            
        }


        public static double hitTimeFloor;
        
        public static void UpdateKeyQueue()
        {
            var key = InputManager.GetKeyCountAsync();
            if (RDC.auto || AudioListener.pause || !Application.isFocused) return;
            
            //if (RDC.auto||AudioListener.pause||scrController.instance.isCLS || !scrController.isGameWorld || scrController.instance.currentState != scrController.States.PlayerControl) return;
            if (key > 0)
            {
                var refAngle = (dspTime - hitTimeFloor);
                for (var n = 0; n < key; n++)
                {

                    keyQueue.Enqueue(refAngle);
                }
            }
            
            
        }


    }
}