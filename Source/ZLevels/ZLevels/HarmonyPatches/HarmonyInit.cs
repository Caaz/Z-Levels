﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ZLevels
{

    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("ZLevels.Mod").PatchAll();
        }

        [HarmonyPatch(typeof(Log))]
        [HarmonyPatch(nameof(Log.Warning))]
        static class Log_Warning_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(string text, ref bool ignoreStopLoggingLimit)
            {
                if (text.Contains("without a specific job end condition") || text.Contains("Drew energy from a PowerNet that didn't have it"))
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Log))]
        [HarmonyPatch(nameof(Log.Error))]
        static class Log_Error_Patch
        {
            public static bool Prefix(string text, ref bool ignoreStopLoggingLimit)
            {
                try
                {
                    if (text != null)
                    {
                        // somehow the game periodically gives this error message when pawns haul between maps
                        // and I really don’t know where the source is and how to fix it. If you know how, then tell me
                        // This error doesnt affect the hauling itself, maybe this error occurs after the completion of the hauling job
                        if (text.Contains("System.Exception: StartCarryThing got availableStackSpace 0 for haulTarg")
                            || text.Contains("overwriting slot group square") // not really an error, this is what z-level needs to look for things for hauling
                            || text.Contains("clearing group grid square") // same
                            || text.Contains("threw exception while executing toil's finish action (0), jobDriver=RimWorld.JobDriver_LayDown")
                            || text.Contains("threw exception while executing toil's finish action (1), jobDriver=RimWorld.JobDriver_LayDown")
                            || text.Contains("Haul designation has no target! Deleting.")
                            )
                        {
                            //ZLogger.Message("The error: " + text);
                            return false;
                        }



                        if (ZLogger.DebugEnabled && Find.TickManager != null)
                        {
                            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {

                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Log))]
        [HarmonyPatch(nameof(Log.Notify_MessageReceivedThreadedInternal))]
        static class Notify_MessageReceivedThreadedInternal_Patch
        {
            public static bool Prefix()
            {
                if (ZLogger.DebugEnabled)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Map))]
        [HarmonyPatch(nameof(Map.ToString))]
        static class Map_ToString
        {
            public static void Postfix(Map __instance, ref string __result)
            {
                if (ZLogger.DebugEnabled)
                {
                    __result = "(" + __result + " - Level " + ZUtils.ZTracker.GetZIndexFor(__instance) + ")";
                }
            }
        }

        [HarmonyPatch(typeof(ColonistBarColonistDrawer), "DrawIcons")]
        public static class DrawIcons_Patch
        {
            public static bool Prefix(Rect rect, Pawn colonist)
            {
                try
                {
                    if (colonist.CurJob != null && colonist.jobs.curDriver.asleep)
                    {

                    }
                }
                catch
                {
                    ZLogger.Pause("Error in JobDriver of " + colonist);
                    colonist.jobs.EndCurrentJob(JobCondition.Errored);
                }
                return true;
            }
        }
    }
}

