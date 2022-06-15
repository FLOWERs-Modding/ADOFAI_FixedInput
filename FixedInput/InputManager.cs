using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityModManagerNet;

namespace FixedInput
{
    public class InputManager
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);
        
        [DllImport("user32.dll")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        

        public static int[] keyCodes;
        
        private static bool[] mask = new bool[256];

        public static HitMargin GetHitMarginFromTime(double refTime, float bpmTimesSpeed, float conductorPitch, double marginScale = 1.0)
        {
            var result = HitMargin.TooEarly;
            var num2 = TimeToAngle(refTime, bpmTimesSpeed);
            var adjustedAngleBoundaryInDeg = scrMisc.GetAdjustedAngleBoundaryInDeg(HitMarginGeneral.Counted, (double)bpmTimesSpeed, (double)conductorPitch, marginScale);
            var adjustedAngleBoundaryInDeg2 = scrMisc.GetAdjustedAngleBoundaryInDeg(HitMarginGeneral.Perfect, (double)bpmTimesSpeed, (double)conductorPitch, marginScale);
            var adjustedAngleBoundaryInDeg3 = scrMisc.GetAdjustedAngleBoundaryInDeg(HitMarginGeneral.Pure, (double)bpmTimesSpeed, (double)conductorPitch, marginScale);
            if ((double)num2 > -adjustedAngleBoundaryInDeg)
            {
                result = HitMargin.VeryEarly;
            }
            if ((double)num2 > -adjustedAngleBoundaryInDeg2)
            {
                result = HitMargin.EarlyPerfect;
            }
            if ((double)num2 > -adjustedAngleBoundaryInDeg3)
            {
                result = HitMargin.Perfect;
            }
            if ((double)num2 > adjustedAngleBoundaryInDeg3)
            {
                result = HitMargin.LatePerfect;
            }
            if ((double)num2 > adjustedAngleBoundaryInDeg2)
            {
                result = HitMargin.VeryLate;
            }
            if ((double)num2 > adjustedAngleBoundaryInDeg)
            {
                result = HitMargin.TooLate;
            }
            return result;
        }
        

        public static double TimeToAngle(double time, double bpmTimesSpeed)
        {
            return (time / scrMisc.bpm2crotchet(bpmTimesSpeed)) * 180;
        }
        

        public static int GetKeyCount()
        {
            var result = 0;
            if (!Application.isFocused) return result;
            if (!Main.KeyKeySetting.useKeyLimit)
            {
                var virKey = new byte[256];
                GetKeyboardState(virKey);
                for (var n = 0; n < 256; n++)
                {
                    
                    if(n==27) continue;
                    if (n >= 16 && n <= 18) continue;
                    if ((virKey[n] & 0x80) != 0)
                    {
                        if (!mask[n])
                        {
                            result++;
                            mask[n] = true;
                        }
                    }
                    else
                    {
                        mask[n] = false;
                    }

                    if (result == 4) break;
                }
            }
            else
            {
                var virKey = new byte[256];
                GetKeyboardState(virKey);
                for (var n = 0; n < keyCodes.Length; n++)
                {
                    var code = keyCodes[n];
                    if ((virKey[code] & 0x80) != 0)
                    {
                        if (!mask[code])
                        {
                            result++;
                            mask[code] = true;
                        }
                    }
                    else
                    {
                        mask[code] = false;
                    }
                    if (result == 4) break;
                }
            }
            return result;
        }
        

        public static void ClearMask()
        {
            for (var n = 0; n < 256; n++)
            {
                if (mask[n])
                    mask[n] = false;
            }
        }
        
        public static int GetKeyCountAsync()
        {
            var result = 0;
            if (RDC.auto || AudioListener.pause) return result;
            if (!Main.KeyKeySetting.useKeyLimit)
            {
                for (var n = 0; n < 256; n++)
                {
                    if(n==27) continue;
                    if (n >= 16 && n <= 18) continue;
                    if ((GetAsyncKeyState(n) & 0x8000) > 0)
                    {
                        if (!mask[n])
                        {
                            result++;
                            mask[n] = true;
                        }
                    }
                    else
                    {
                        mask[n] = false;
                    }

                    if (result == 4) break;
                }
            }
            else
            {
                for (var n = 0; n < keyCodes.Length; n++)
                {
                    var code = keyCodes[n];
                    if ((GetAsyncKeyState(code) & 0x8000) > 0)
                    {
                        if (AudioListener.pause || RDC.auto) continue;
                        if (!mask[code])
                        {
                            result++;
                            mask[code] = true;
                        }
                    }
                    else
                    {
                        mask[code] = false;
                    }
                    if (result == 4) break;
                }
            }
            return result;
        }
    }
}