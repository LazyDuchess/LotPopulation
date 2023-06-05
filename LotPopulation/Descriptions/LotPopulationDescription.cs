using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class LotPopulationDescription
    {
        [PersistableStatic]
        static Dictionary<Sim, bool> simsOnLowPriority = new Dictionary<Sim, bool>();

        public static LotPopulationDescription Singleton = new LotPopulationDescription();
        
        // Chances of spawning indoors vs outdoors. 100 means always indoors, 0 means always outdoors.
        public virtual float IndoorSpawnChance => 100f;

        // If lot is outdoorsy, makes on-screen behavior a little more strict to make pop-in less obvious.
        public virtual bool Outdoorsy => false;

        // Parks and beaches tend to naturally extend a little past the lot boundary. Beaches especially due to the shore. This accounts for it for population.
        public virtual float OutdoorSizePastBoundary => 0f;

        public static void MakeSimLowPriority(Sim sim)
        {
            if (Main.kSimLowPriorityDuration <= 0f)
                return;
            simsOnLowPriority[sim] = true;
            AlarmManager.Global.AddAlarm(Main.kSimLowPriorityDuration, TimeUnit.Hours, new AlarmTimerCallback(() =>
            {
                simsOnLowPriority.Remove(sim);
            }), "Sim Population Cooldown", AlarmType.AlwaysPersisted, null);
        }

        public static bool IsSimLowPriority(Sim sim)
        {
            if (simsOnLowPriority.TryGetValue(sim, out bool _))
                return true;
            return false;
        }

        public struct CooldownDescription
        {
            public float Time;
            public TimeUnit Unit;

            public CooldownDescription(float time, TimeUnit timeUnit)
            {
                Time = time;
                Unit = timeUnit;
            }
        }

        public virtual CooldownDescription GetCooldown(Lot lot)
        {
            return new CooldownDescription(1f, TimeUnit.Hours);
        }

        protected bool MakeSimDoRandomActionForCommodity(Sim sim, CommodityKind commodity, Lot lot)
        {
            var actions = new List<InteractionInstance>();
            try
            {
                actions = sim.Autonomy.FindActionsForCommodity(commodity, 10, AutonomySearchType.Autonomy);
            }
            catch(Exception e)
            {
                Debug.NotifyLot($"Error when finding action for Sim {sim.FullName}, commodity {commodity}.\n{e}", lot);
            }
            if (actions.Count <= 0)
                return false;
            var action = RandomUtil.GetRandomObjectFromList(actions);
            if (action != null)
            {
                sim.InteractionQueue.AddNext(action);
                return true;
            }
            return false;
        }

        public virtual void SetupSim(Sim sim, Lot lot)
        {
            sim.Motives.SetValue(CommodityKind.Social, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.Fun, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.Hunger, RandomUtil.GetFloat(-10f, 100f));
            sim.Motives.SetValue(CommodityKind.StayAtVenue, 100f);
            if (RandomUtil.CoinFlip())
                MakeSimDoRandomActionForCommodity(sim, CommodityKind.Fun, lot);
            else
                MakeSimDoRandomActionForCommodity(sim, CommodityKind.Social, lot);
            sim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday);
        }

        public virtual int GetAmountOfPeopleForLot(Lot lot)
        {
            var baseline = Math.Min(20, Demographics.GetBaselineLotPopulation());
            if (TimeUtils.IsTherePrecipitation)
                baseline = (int)(baseline * 0.5);
            if (TimeUtils.IsNight)
                return 0;
            if (TimeUtils.IsEarlyMorning)
                return 0;
            if (TimeUtils.IsMorning)
            {
                if (TimeUtils.IsTodayWeekendOrHoliday)
                    return (int)(baseline * 0.4);
                return 0;
            }
                
            if (TimeUtils.IsGettingLate)
            {
                if (TimeUtils.IsTomorrowWeekendOrHoliday)
                    return (int)(baseline * 0.8);
                else
                    return (int)(baseline * 0.4);
            }
            return 10;
        }
        public virtual int GetWeightForSim(Sim sim)
        {
            if (!sim.SimDescription.TeenOrAbove)
                return 0;
            var weight = 10;
            if (sim.SimDescription.Adult)
                weight = 5;
            if (sim.SimDescription.Elder)
                weight = 1;

            if (sim.TraitManager.HasElement(TraitNames.Shy))
                return 1;
            if (sim.TraitManager.HasElement(TraitNames.Friendly))
                weight += 5;
            if (sim.TraitManager.HasElement(TraitNames.CouchPotato))
                weight -= 5;
            return weight;
        }

        public static bool IsSimInLotWithASelectableSim(Sim sim)
        {
            if (sim.IsOutside)
                return false;
            if (Household.ActiveHousehold == null)
                return false;
            foreach (var otherSim in Household.ActiveHousehold.Sims)
            {
                if (otherSim.LotCurrent == sim.LotCurrent)
                    return true;
                foreach(var interaction in otherSim.InteractionQueue.InteractionList)
                {
                    if (interaction.GetTargetLot() == sim.LotCurrent)
                        return true;
                }
            }
            return false;
        }

        public virtual void OnArrive(Sim sim, float x)
        {

        }

        protected virtual bool CheckSimForVisit(Sim sim)
        {
            if (sim.MoodManager.IsInNegativeMood)
                return false;
            return true;
        }

        public List<Sim> GetWeightedVisitCandidates(Lot lot)
        {
            var candidates = GetVisitCandidates(lot);
            var weightedCandidates = new List<Sim>();
            foreach (var candidate in candidates)
            {
                var weight = GetWeightForSim(candidate);
                if (weight <= 0)
                    weight = 1;
                for (var i = 0; i < weight; i++)
                {
                    weightedCandidates.Add(candidate);
                }
            }
            return weightedCandidates;
        }

        List<Sim> GetVisitCandidates(Lot lot)
        {
            var candidates = new List<Sim>();
            foreach (var sim in LotManager.Actors)
            {
                if (Main.kOnlyTeleportSimsDoingNothing)
                {
                    if (sim.CurrentInteraction != null)
                        continue;
                }
                if (sim.LotCurrent == lot)
                    continue;
                if (sim.IsSocializing)
                    continue;
                if (sim.IsGhostOrHasGhostBuff)
                    continue;
                if (!sim.IsHuman)
                    continue;
                if (sim.IsAtWork)
                    continue;
                if (sim.IsDying())
                    continue;
                if (sim.IsPerformingAService)
                    continue;
                if (!sim.IsStandingIdle)
                    continue;
                if (sim.IsSelectable)
                    continue;
                if (sim.IsSleeping)
                    continue;
                if (IsSimInLotWithASelectableSim(sim))
                    continue;
                if (sim.GetSituationOfType<Situation>() != null)
                    continue;
                if (sim.SimDescription.HasActiveRole)
                    continue;
                if (IsSimInCurfew(sim))
                    continue;
                if (sim.RabbitHoleCurrent != null)
                    continue;
                if (Utils.IsSimPerforming(sim))
                    continue;
                if (sim.BuffManager.HasElement(BuffNames.Singed))
                    continue;
                if (sim.BuffManager.HasElement(BuffNames.SingedElectricity))
                    continue;
                if (sim.SimDescription.IsVampire && !SimClock.IsNightTime())
                    continue;
                if (Household.ActiveHouseholdLot != null)
                {
                    if (sim.LotCurrent == Household.ActiveHouseholdLot || sim.LotHome == Household.ActiveHouseholdLot)
                        continue;
                }
                if (Household.ActiveHousehold != null)
                {
                    if (sim.Household == Household.ActiveHousehold)
                        continue;
                }
                if (!CheckSimForVisit(sim))
                    continue;
                var weight = GetWeightForSim(sim);
                if (weight <= 0)
                    continue;
                candidates.Add(sim);
            }
            return candidates;
        }

        public static bool IsSimInCurfew(Sim sim)
        {
            if (sim.SimDescription.YoungAdultOrAbove)
                return false;
            if (sim.SimDescription.ToddlerOrBelow)
                return false;
            if (sim.SimDescription.Child)
            {
                if (SimClock.IsTimeBetweenTimes(Sim.kChildStartCurfewHour, Sim.kChildEndCurfewHour))
                    return true;
            }
            if (sim.SimDescription.Teen)
            {
                if (SimClock.IsTimeBetweenTimes(Sim.kTeenStartCurfewHour, Sim.kTeenEndCurfewHour))
                    return true;
            }
            return false;
        }

        public static bool IsSimOpportunityTarget(Sim sim)
        {
            if (Household.ActiveHousehold == null)
                return false;
            foreach(var otherSim in Household.ActiveHousehold.Sims)
            {
                if (otherSim.HasOpportunity())
                {
                    foreach(var opportunity in otherSim.OpportunityManager.List)
                    {
                        if (opportunity.TargetObject == sim)
                            return true;
                    }
                }
            }
            return false;
        }

        public List<Sim> GetPopulationCandidates(Lot lot, out List<Sim> lowPriorityCandidates)
        {
            var candidates = new List<Sim>();
            lowPriorityCandidates = new List<Sim>();
            foreach (var sim in LotManager.Actors)
            {
                if (Main.kOnlyTeleportSimsDoingNothing)
                {
                    if (sim.CurrentInteraction != null)
                        continue;
                }
                if (sim.LotCurrent == lot)
                    continue;
                if (sim.IsSocializing)
                    continue;
                if (sim.IsGhostOrHasGhostBuff)
                    continue;
                if (!sim.IsHuman)
                    continue;
                if (sim.IsAtWork)
                    continue;
                if (sim.IsDying())
                    continue;
                if (sim.IsPerformingAService)
                    continue;
                if (!sim.IsStandingIdle)
                    continue;
                if (sim.IsSelectable)
                    continue;
                if (sim.IsSleeping)
                    continue;
                if (IsSimInLotWithASelectableSim(sim))
                    continue;
                if (sim.GetSituationOfType<Situation>() != null)
                    continue;
                if (sim.SimDescription.HasActiveRole)
                    continue;
                if (IsSimOpportunityTarget(sim))
                    continue;
                if (IsSimInCurfew(sim))
                    continue;
                if (sim.RabbitHoleCurrent != null)
                    continue;
                if (Utils.IsSimPerforming(sim))
                    continue;
                if (sim.Parent != null)
                    continue;
                if (sim.SimDescription.IsVampire && !SimClock.IsNightTime())
                    continue;
                if (!Utils.IsCameraInTheSky())
                {
                    if (sim.IsOutside)
                    {
                        if (World.IsObjectOnScreen(sim))
                            continue;
                    }
                    else
                    {
                        if (sim.LotCurrent != null && sim.LotCurrent != LotManager.sWorldLot)
                        {
                            if (Utils.IsLotOnScreen(sim.LotCurrent) && LotManager.ActiveLot == sim.LotCurrent)
                                continue;
                        }
                    }
                }
                if (Household.ActiveHouseholdLot != null)
                {
                    if (sim.LotCurrent == Household.ActiveHouseholdLot || sim.LotHome == Household.ActiveHouseholdLot)
                        continue;
                }
                if (Household.ActiveHousehold != null)
                {
                    if (sim.Household == Household.ActiveHousehold)
                        continue;
                }
                var weight = GetWeightForSim(sim);
                if (weight <= 0)
                    continue;
                if (IsSimLowPriority(sim))
                    lowPriorityCandidates.Add(sim);
                else
                    candidates.Add(sim);
            }
            return candidates;
        }
    }
}
