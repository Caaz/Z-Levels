﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ZLevels
{

    [HarmonyPatch(typeof(ReachabilityUtility), "CanReach")]
    public class CanReach_Patch
    {
        private static void Postfix(ref bool __result, Pawn pawn, LocalTargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBash = false, TraverseMode mode = TraverseMode.ByPawn)
        {
            if (!__result && pawn.RaceProps.Humanlike)
            {
                if (dest.HasThing)
                {
                    Log.Message($"{pawn} can reach {dest}: {__result}, {pawn.Map} - {dest.Thing.Map}");
                }
                if (dest.HasThing && dest.thingInt.Map != pawn.Map)
                {
                    ZLogger.Pause($"CanReach: Detected reachabiity disfunction: pawn: {pawn}, thing: {dest.thingInt}, pawn.Map: {pawn.Map}, thing.Map: {dest.thingInt.Map}");
                }
            }
        }
    }
    [HarmonyPatch(typeof(ReservationManager))]
    [HarmonyPatch("CanReserve")]
    public class ReservationManager_Patch
    {
        private static void Postfix(ref bool __result, Pawn claimant, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
        {
            if (__result)
            {
                var thing = target.Thing;
                if (thing != null)
                {
                    if (claimant.RaceProps.Humanlike && target.HasThing)
                    {
                        var ZTracker = ZUtils.ZTracker;
                        //foreach (var map in ZTracker.GetAllMaps(claimant.Map.Tile))
                        //{
                        //    foreach (var reservation in map.reservationManager.reservations)
                        //    {
                        //        Log.Message($"map: {map}, Reservation: {reservation.claimant}, target: {reservation.target}, {reservation.claimant.Map} - {reservation.target.Thing?.Map}");
                        //    }
                        //}

                        if (ZTracker.jobTracker != null)
                        {
                            foreach (var data in ZTracker.jobTracker)
                            {

                                if (data.Key != claimant && data.Value.reservedThings != null)
                                {
                                    foreach (var reservation in data.Value.reservedThings)
                                    {
                                        if (reservation.HasThing && reservation.thingInt == target.thingInt)
                                        {
                                            __result = false;
                                            ZLogger.Message($"Detected ZTRACKER reservation disfunction: claimant: {claimant}, pawn: {data.Key}, thing: {thing}");
                                            return;
                                        }
                                        //Log.Message($"ZTracker reservation: map: Reservation: {data.Key}, target: {reservation}, {data.Key.Map} - {reservation.Thing?.Map}");
                                    }
                                }
                            }
                        }
                        //Log.Message($"---------------------");
                    }
                    //Log.Message($"{claimant} can reserve {target}: {__result}, {claimant.Map} - {target.Thing?.Map}");

                    //if (claimant.Map != thing.Map)
                    //{
                    //    ZLogger.Pause($"CanReserve: {__result}, Detected reservation disfunction: claimant.Map != thing.Map, claimant: {claimant}, thing: {thing}");
                    //    var ZTracker = ZUtils.ZTracker;
                    //    foreach (var map in ZTracker.GetAllMaps(thing.Map.Tile))
                    //    {
                    //        var pawn = map.reservationManager.FirstRespectedReserver(target, claimant);
                    //        if (pawn != null && pawn != claimant)
                    //        {
                    //            ZLogger.Pause($"CanReserve: {__result}, Detected reservation disfunction: claimant: {claimant}, pawn: {pawn}, thing: {thing}");
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    var ZTracker = ZUtils.ZTracker;
                    //    foreach (var map in ZTracker.GetAllMaps(thing.Map.Tile))
                    //    {
                    //        var pawn = map.reservationManager.FirstRespectedReserver(target, claimant);
                    //        if (pawn != null && pawn != claimant)
                    //        {
                    //            ZLogger.Pause($"CanReserve: {__result}, Detected other claimant: first claimant: {claimant}, second claimant: {pawn}, thing: {thing}");
                    //        }
                    //    }
                    //}
                }
            }
        }
    }

    //[HarmonyPatch(typeof(ReservationManager))]
    //[HarmonyPatch("Reserve")]
    //public class ReservationManager_Patch_Reserve
    //{
    //    private static void Postfix(bool __result, Pawn claimant, Job job, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool errorOnFailed = true)
    //    {
    //        if (__result && claimant.RaceProps.Humanlike)
    //        {
    //            Log.Message($"{claimant} is reserving {target}: {__result}, {claimant.Map} - {target.Thing?.Map}");
    //            var ZTracker = ZUtils.ZTracker;
    //            foreach (var map in ZTracker.GetAllMaps(claimant.Map.Tile))
    //            {
    //                foreach (var reservation in map.reservationManager.reservations)
    //                {
    //                    Log.Message($"Vanilla reservation: map: {map}, Reservation: {reservation.claimant}, target: {reservation.target}, {reservation.claimant.Map} - {reservation.target.Thing?.Map}");
    //                }
    //            }
    //            if (ZTracker.jobTracker != null)
    //            {
    //                foreach (var data in ZTracker.jobTracker)
    //                {
    //                    if (data.Value.reservedThings != null)
    //                    {
    //                        foreach (var reservation in data.Value.reservedThings)
    //                        {
    //                            Log.Message($"ZTracker reservation: map: Reservation: {data.Key}, target: {reservation}, {data.Key.Map} - {reservation.Thing?.Map}");
    //                        }
    //                    }
    //                }
    //            }
    //
    //            Log.Message($"---------------------");
    //
    //        }
    //    }
    //}
}