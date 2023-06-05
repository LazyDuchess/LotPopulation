using LazyDuchess.LotPopulation.Descriptions;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation
{
    public static class LotPopulator
    {
        static Dictionary<Lot, bool> lotsBeingPopulated = new Dictionary<Lot, bool>();
        public static bool Busy = false;

        public class Settings
        {
            public static Settings Default = new Settings();
            public bool IgnoreOnScreenChecks = false;

            public Settings()
            {

            }
            public Settings(bool ignoreOnScreenChecks)
            {
                IgnoreOnScreenChecks = ignoreOnScreenChecks;
            }
        }

        public static bool IsLotBeingPopulated(Lot lot)
        {
            return lotsBeingPopulated.TryGetValue(lot, out bool _);
        }

        public static LotPopulationDescription GetDescriptionForLot(Lot lot)
        {
            switch (lot.CommercialLotSubType)
            {
                case CommercialLotSubType.kHospital:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP8_Festival:
                    return FestivalPopulationDescription.Singleton;

                case CommercialLotSubType.kLibrary:
                    return LibraryPopulationDescription.Singleton;

                case CommercialLotSubType.kGym:
                    return GymPopulationDescription.Singleton;

                case CommercialLotSubType.kPool:
                    return PoolPopulationDescription.Singleton;

                case CommercialLotSubType.kBeach:
                    return BeachPopulationDescription.Singleton;

                case CommercialLotSubType.kEP9_RebelHangout:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP9_UniversityHangout:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kHangout:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP7_WerewolfBar:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_CocktailLoungeAsian:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_DiveBarSports:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_DiveBarIrish:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_DiveBarCriminal:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_CocktailLoungeVampire:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_CocktailLoungeCelebrity:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_DanceClubRave:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_DanceClubPool:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP3_DanceClubLiveMusic:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP11_FutureBar:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP6_PrivateVenue:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP6_PerformanceClub:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP6_BigShow:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kEP6_Bistro:
                    return NightlifePopulationDescription.Singleton;

                case CommercialLotSubType.kSmallPark:
                    return OutdoorPopulationDescription.Singleton;

                case CommercialLotSubType.kBigPark:
                    return OutdoorPopulationDescription.Singleton;

                case CommercialLotSubType.kEP5_DogPark:
                    return OutdoorPopulationDescription.Singleton;

                case CommercialLotSubType.kEP11_RecreationPark:
                    return OutdoorPopulationDescription.Singleton;

                case CommercialLotSubType.kGraveyard:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kFishingSpot:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kMisc_Visitors:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kMisc_NoVisitors:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP2_JunkyardNoVisitors:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP2_Junkyard:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP5_Junkyard:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP2_FireStation:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP5_FireStation:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP10_Diving:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP10_Port:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP1_LandmarkTomb:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP1_HiddenTomb:
                    return EmptyPopulationDescription.Singleton;

                case CommercialLotSubType.kEP11_HoverTrainStop:
                    return EmptyPopulationDescription.Singleton;
            }
            return LotPopulationDescription.Singleton;
        }

        static Sim GetRandomSimAtLot(Lot lot)
        {
            var sims = lot.GetSims(new Predicate<Sim>((Sim target) => target.IsHuman));
            if (sims.Count <= 0)
                return null;
            return RandomUtil.GetRandomObjectFromList(sims);
        }

        static List<Vector3> GetIndoorSpawnLocationsForLot(Sim sim, Lot lot)
        {
            var referenceSim = sim;
            if (Main.kUseReferenceSimFromLot)
            {
                referenceSim = GetRandomSimAtLot(lot);
                if (referenceSim == null)
                    referenceSim = sim;
            }
            var indoorPositionslot = new List<Vector3>(lot.GetRoutableIndoorPositions(referenceSim, 10).Keys);
            return indoorPositionslot;
        }

        static List<Vector3> GetOutdoorSpawnLocationsForLot(Sim sim, Lot lot)
        {
            var referenceSim = sim;
            if (Main.kUseReferenceSimFromLot)
            {
                referenceSim = GetRandomSimAtLot(lot);
                if (referenceSim == null)
                    referenceSim = sim;
            }
            var iterations = 50;
            var outdoorPositionsLot = new List<Vector3>();
            for (var i = 0; i < iterations; i++)
            {
                var position = World.SnapToFloor(lot.GetRandomPosition(false, true));
                if (referenceSim.CanRouteToPoint(position))
                    outdoorPositionsLot.Add(position);
            }
            return outdoorPositionsLot;
        }

        public static void PopulateLotAsync(Lot lot, Settings settings)
        {
            Simulator.AddObject(new Sims3.Gameplay.OneShotFunctionTask(() => {
                if (IsLotBeingPopulated(lot))
                    return;
                lotsBeingPopulated[lot] = true;
                if (Main.kWaitForPopulationToFinish && Busy && Simulator.CheckYieldingContext(false))
                {
                    Debug.NotifyLot("Busy, gonna wait.", lot);
                    while (Busy)
                        Simulator.Sleep(0);
                }
                Busy = true;
                try
                {
                    PopulateLot(lot, settings);
                }
                catch(Exception e)
                {
                    Debug.NotifyLot($"Population error!\n{e}", lot);
                }
                if (IsLotBeingPopulated(lot))
                    lotsBeingPopulated.Remove(lot);
                Busy = false;
            }));
        }

        public static bool CheckSimIsNormalVisitor(Sim sim)
        {
            if (sim.GetSituationOfType<Situation>() != null)
                return false;
            if (sim.IsPerformingAService)
                return false;
            if (sim.SimDescription.HasActiveRole)
                return false;
            if (sim.IsGhostOrHasGhostBuff)
                return false;
            return true;
        }

        public static int GetSimsGoingToOrAtLot(Lot lot)
        {
            var description = GetDescriptionForLot(lot);
            var num = 0;
            foreach(var sim in LotManager.Actors)
            {
                if (!CheckSimIsNormalVisitor(sim))
                    continue;
                if (sim.LotCurrent == lot)
                    num++;
                else
                {
                    var inLot = false;
                    foreach (var interaction in sim.InteractionQueue.InteractionList)
                    {
                        if (interaction.GetTargetLot() == lot)
                        {
                            inLot = true;
                            break;
                        }
                    }
                    if (inLot)
                        num++;
                    else
                    {
                        if (description.Outdoorsy && description.OutdoorSizePastBoundary > 0f)
                        {
                            if (lot.GetDistanceToLotBoundary(sim.Position) <= description.OutdoorSizePastBoundary)
                                num++;
                        }
                    }
                }

            }
            return num;
        }

        public static void PopulateLot(Lot lot, Settings settings)
        {
            var description = GetDescriptionForLot(lot);
            var people = description.GetAmountOfPeopleForLot(lot);
            var peopleAlreadyInLot = GetSimsGoingToOrAtLot(lot);
            if (peopleAlreadyInLot >= people)
            {
                Debug.NotifyLot("Already enough people in lot.", lot);
                return;
            }
            people -= peopleAlreadyInLot;
            var candidates = description.GetPopulationCandidates(lot, out List<Sim> lowPriorityCandidates);
            var simsToSpawn = new List<Sim>();
            if (candidates.Count < people)
            {
                Debug.NotifyLot("Using low priority Sims to fill quota.", lot);
                candidates.AddRange(lowPriorityCandidates);
            }
            if (candidates.Count <= people)
            {
                simsToSpawn = candidates;
            }
            else
            {
                var weightedCandidates = new List<Sim>();
                foreach (var candidate in candidates)
                {
                    var weight = description.GetWeightForSim(candidate);
                    if (weight <= 0)
                        weight = 1;
                    for (var i = 0; i < weight; i++)
                    {
                        weightedCandidates.Add(candidate);
                    }
                }
                for (var i = 0; i < people; i++)
                {
                    if (weightedCandidates.Count <= 0)
                    {
                        Debug.NotifyLot($"Somehow weighted candidates is empty before we could pick all Sims to spawn. We wanted {people} people, got {candidates.Count} candidates.", lot);
                    }
                    try
                    {
                        var sim = RandomUtil.GetRandomObjectFromList(weightedCandidates);
                        simsToSpawn.Add(sim);
                        weightedCandidates.RemoveAll(new Predicate<Sim>((Sim target) => { return target.ObjectId == sim.ObjectId; }));
                    }
                    catch(Exception e)
                    {
                        Debug.NotifyLot($"Problem getting random Sim from weightedCandidates. weightedCandidates.Count: {weightedCandidates.Count}\n{e}", lot);
                    }
                }
            }

            if (simsToSpawn.Count <= 0)
            {
                Debug.NotifyLot("Can't find enough Sims to spawn.", lot);
                return;
            }

            var indoorLocations = GetIndoorSpawnLocationsForLot(simsToSpawn[0], lot);
            var outdoorLocations = GetOutdoorSpawnLocationsForLot(simsToSpawn[0], lot);

            Debug.NotifyLot($"Indoor Locations: {indoorLocations.Count}, Outdoor locations: {outdoorLocations.Count}", lot);

            var canSpawnIndoor = indoorLocations.Count > 0;
            var canSpawnOutdoor = outdoorLocations.Count > 0;
            if (!settings.IgnoreOnScreenChecks)
            {
                if (Utils.IsLotOnScreen(lot) && !Utils.IsCameraInTheSky())
                    canSpawnOutdoor = false;
            }
            var iters = 0;
            foreach (var sim in simsToSpawn)
            {
                var spawnIndoor = RandomUtil.RandomChance(description.IndoorSpawnChance);
                if (!spawnIndoor)
                {
                    if (!canSpawnOutdoor)
                    {
                        spawnIndoor = true;
                        if (!canSpawnIndoor)
                            break;
                    }
                }
                else
                {
                    if (!canSpawnIndoor)
                    {
                        spawnIndoor = false;
                        if (!canSpawnOutdoor)
                            break;
                    }
                }
                var position = Vector3.Invalid;
                if (spawnIndoor)
                {
                    try
                    {
                        position = RandomUtil.GetRandomObjectFromList(indoorLocations);
                        indoorLocations.Remove(position);
                    }
                    catch (Exception e)
                    {
                        Debug.NotifyLot($"Problem getting random indoor position. indoorLocations.Count: {indoorLocations.Count}\n{e}", lot);
                        indoorLocations.Clear();
                    }
                    if (indoorLocations.Count <= 0)
                    {
                        indoorLocations = GetIndoorSpawnLocationsForLot(sim, lot);
                        if (indoorLocations.Count <= 0)
                            canSpawnIndoor = false;
                    }
                    
                }
                else
                {
                    try
                    {
                        position = RandomUtil.GetRandomObjectFromList(outdoorLocations);
                        outdoorLocations.Remove(position);
                    }
                    catch(Exception e)
                    {
                        Debug.NotifyLot($"Problem getting random outdoor position. outdoorLocations.Count: {outdoorLocations.Count}\n{e}", lot);
                        outdoorLocations.Clear();
                    }
                    if (outdoorLocations.Count <= 0)
                    {
                        outdoorLocations = GetOutdoorSpawnLocationsForLot(sim, lot);
                        if (outdoorLocations.Count <= 0)
                            canSpawnOutdoor = false;
                    }
                }
                LotPopulationDescription.MakeSimLowPriority(sim);
                sim.GreetSimOnLot(lot);
                sim.SetPosition(position);
                sim.PopPosture();
                Sim.RemoveGoHomeInteractions(sim, false);
                sim.Motives.RecreateMotives(sim);
                sim.SetObjectToReset();
                sim.BuffManager.RemoveElement(BuffNames.Singed);
                sim.BuffManager.RemoveElement(BuffNames.SingedElectricity);
                if (Simulator.CheckYieldingContext(false))
                    Simulator.Sleep(0);
                description.SetupSim(sim, lot);
                iters++;
            }
            Debug.NotifyLot($"Stopped spawning after {iters} Sims. Was going for {people}, sims to spawn were {simsToSpawn.Count}", lot);
        }
    }
}
