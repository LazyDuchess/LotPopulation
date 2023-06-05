using Sims3.Gameplay;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation
{
    public class LotPopulationManager : Task
    {
        static LotPopulationManager Singleton = null;

        [PersistableStatic]
        static Dictionary<Lot, AlarmHandle> lotsRecentlyPopulated = new Dictionary<Lot, AlarmHandle>();
        static Lot lastActiveLot = LotManager.sWorldLot;
        static Dictionary<Lot, bool> lotsInCameraDistance = new Dictionary<Lot, bool>();
        public static List<Lot> LotsInCameraDistance => new List<Lot>(lotsInCameraDistance.Keys);

        public static void Start()
        {
            lastActiveLot = LotManager.ActiveLot;
            if (lastActiveLot == null)
                lastActiveLot = LotManager.sWorldLot;

            if (Main.kPopulateOutdoorLotsByCameraDistance)
            {
                var lots = LotManager.GetAllCommunityLots();
                if (lots != null)
                {
                    foreach (var lot in lots)
                    {
                        if (lot == null)
                            continue;
                        if (IsLotInCameraDistance(lot))
                            CacheLotInCameraDistance(lot);
                    }
                }
            }

            Singleton = new LotPopulationManager();
            Simulator.AddObject(Singleton);
        }

        public static void Shutdown()
        {
            if (Singleton == null)
                return;
            Singleton.Stop();
            Singleton.Dispose();
            Simulator.DestroyObject(Singleton.ObjectId);
            Singleton = null;
        }

        // Add a cooldown to lots just for looking at them, so that we don't magically spawn Sims that weren't there 1 second ago when paused.
        static void DoLookAtCooldown(Lot lot)
        {
            if (!Main.kLookingAtCooldownEnabled)
                return;
            var time = Main.kLookingAtLotCooldown;
            if (!IsLotOnCooldown(lot))
            {
                lotsRecentlyPopulated[lot] = AlarmManager.Global.AddAlarm(time, TimeUnit.Minutes, new AlarmTimerCallback(() =>
                {
                    lotsRecentlyPopulated.Remove(lot);
                }), "Lot Population Cooldown", AlarmType.AlwaysPersisted, null);
            }
            else
            {
                var cd = lotsRecentlyPopulated[lot];
                var timeLeft = SimClock.ConvertToTicks(AlarmManager.Global.GetTimeLeft(cd, TimeUnit.Seconds), TimeUnit.Seconds);
                var newTimeInTicks = SimClock.ConvertToTicks(time, TimeUnit.Minutes);
                if (newTimeInTicks >= timeLeft)
                {
                    AlarmManager.Global.RemoveAlarm(cd);
                    lotsRecentlyPopulated[lot] = AlarmManager.Global.AddAlarm(time, TimeUnit.Minutes, new AlarmTimerCallback(() =>
                    {
                        lotsRecentlyPopulated.Remove(lot);
                    }), "Lot Population Cooldown", AlarmType.AlwaysPersisted, null);
                }
            }
        }

        static void CooldownLot(Lot lot)
        {
            var description = LotPopulator.GetDescriptionForLot(lot);
            var cooldown = description.GetCooldown(lot);
            lotsRecentlyPopulated[lot] = AlarmManager.Global.AddAlarm(cooldown.Time, cooldown.Unit, new AlarmTimerCallback(() =>
            {
                lotsRecentlyPopulated.Remove(lot);
            }), "Lot Population Cooldown", AlarmType.AlwaysPersisted, null);
        }

        public static bool IsLotOnCooldown(Lot lot)
        {
            if (lotsRecentlyPopulated.TryGetValue(lot, out AlarmHandle _))
                return true;
            return false;
        }

        bool AnySimFromCurrentHouseholdInLot(Lot lot)
        {
            if (Household.ActiveHousehold == null)
                return false;
            foreach (var sim in Household.ActiveHousehold.Sims)
            {
                if (sim.LotCurrent == lot)
                    return true;
            }
            return false;
        }

        static void PopulateAndCooldownLotDontCheckScreen(Lot lot, LotPopulator.Settings settings)
        {
            if (IsLotOnCooldown(lot))
                return;
            if (!lot.IsOpenVenue())
                return;
            if (Main.kOnlyPopulateInLiveMode && !GameStates.IsLiveState)
                return;
            if (GameStates.IsPlayFlowState || GameStates.IsEditTownState)
                return;
            if (lot.LotType == LotType.Commercial)
            {
                LotPopulator.PopulateLotAsync(lot, settings);
                CooldownLot(lot);
            }
        }

        static void PopulateAndCooldownLotIfValid(Lot lot, LotPopulator.Settings settings)
        {
            if (IsLotOnCooldown(lot))
                return;
            if (!lot.IsOpenVenue())
                return;
            if (Main.kOnlyPopulateInLiveMode && !GameStates.IsLiveState)
                return;
            if (GameStates.IsPlayFlowState || GameStates.IsEditTownState)
                return;
            if (lot.LotType == LotType.Commercial)
            {
                var lotDescription = LotPopulator.GetDescriptionForLot(lot);
                if (!settings.IgnoreOnScreenChecks)
                {
                    if (lotDescription.Outdoorsy)
                    {
                        if (Utils.IsLotOnScreen(lot) && !Utils.IsCameraInTheSky())
                            return;
                    }
                    else
                    {
                        if (Utils.IsLotOnScreen(lot) && LotManager.ActiveLot == lot)
                            return;
                    }
                }
                LotPopulator.PopulateLotAsync(lot, settings);
                CooldownLot(lot);
            }
        }

        static void RemoveLotFromInCameraDistanceCache(Lot lot)
        {
            if (lotsInCameraDistance.TryGetValue(lot, out bool _))
                lotsInCameraDistance.Remove(lot);
        }

        static void CacheLotInCameraDistance(Lot lot)
        {
            lotsInCameraDistance[lot] = true;
        }

        static bool WasLotAlreadyInCameraDistance(Lot lot)
        {
            if (lotsInCameraDistance.TryGetValue(lot, out bool _))
                return true;
            return false;
        }

        static bool IsLotInCameraDistance(Lot lot)
        {
            if (!CameraController.IsMapViewModeEnabled())
            {
                if (Main.kOutdoorLotCameraDistanceMinimum > 0f)
                {
                    var cameraPositionOnFloor = World.SnapToFloor(CameraController.GetPosition());
                    var cameraDistance = lot.GetDistanceToLotBoundary(cameraPositionOnFloor);
                    if (cameraDistance <= Main.kOutdoorLotCameraDistanceMinimum)
                        return true;
                }
            }
            else
            {
                if (Main.kOutdoorLotMapViewDistanceMinimum > 0f)
                {
                    var cameraDistance = lot.GetDistanceToLotBoundary(CameraController.GetTarget());
                    if (cameraDistance <= Main.kOutdoorLotMapViewDistanceMinimum)
                        return true;
                }
            }
            return false;
        }

        public override void Simulate()
        {
            var currentActiveLot = LotManager.ActiveLot;
            if (currentActiveLot == null)
                currentActiveLot = LotManager.sWorldLot;
            if (currentActiveLot != lastActiveLot && currentActiveLot != LotManager.sWorldLot && Main.kPopulateLotsOnLookInside)
            {
                lastActiveLot = currentActiveLot;
                var popSettings = LotPopulator.Settings.Default;
                if (!WasLotAlreadyInCameraDistance(currentActiveLot) || !IsLotInCameraDistance(currentActiveLot))
                    popSettings = new LotPopulator.Settings(true);
                PopulateAndCooldownLotDontCheckScreen(currentActiveLot, popSettings);
            }
            if (currentActiveLot != LotManager.sWorldLot)
                DoLookAtCooldown(currentActiveLot);

            var lots = LotManager.GetAllCommunityLots();
            foreach (var lot in lots)
            {
                var description = LotPopulator.GetDescriptionForLot(lot);
                if (IsLotInCameraDistance(lot))
                {
                    if (!WasLotAlreadyInCameraDistance(lot))
                    {
                        if (description.Outdoorsy && Main.kPopulateOutdoorLotsByCameraDistance)
                            PopulateAndCooldownLotIfValid(lot, new LotPopulator.Settings(true));
                        CacheLotInCameraDistance(lot);
                    }
                    if (!Utils.IsCameraInTheSky() && description.Outdoorsy)
                         DoLookAtCooldown(lot);
                }
                else
                {
                    RemoveLotFromInCameraDistanceCache(lot);
                }
            }
            if (Household.ActiveHousehold == null)
                return;
            foreach(var sim in Household.ActiveHousehold.Sims)
            {
                Lot targetLot = null;
                if (sim.CurrentInteraction != null)
                    targetLot = sim.CurrentInteraction.GetTargetLot();
                if (targetLot == null)
                    targetLot = sim.LotCurrent;
                if (targetLot != null && targetLot != LotManager.sWorldLot)
                {
                    PopulateAndCooldownLotIfValid(targetLot, LotPopulator.Settings.Default);
                }
            }
        }
    }
}
