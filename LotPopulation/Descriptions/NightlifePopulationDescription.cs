using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class NightlifePopulationDescription : LotPopulationDescription
    {
        public static LotPopulationDescription Singleton = new NightlifePopulationDescription();

        public override int GetWeightForSim(Sim sim)
        {
            if (sim.SimDescription.IsPregnant)
                return 0;
            if (!sim.SimDescription.YoungAdultOrAbove)
                return 0;

            var weight = 10;
            if (sim.SimDescription.Adult)
                weight = 5;
            if (sim.SimDescription.Elder)
                weight = 1;

            if (sim.TraitManager.HasElement(TraitNames.Shy))
                return 1;
            if (sim.TraitManager.HasElement(TraitNames.PartyAnimal))
                weight += 10;
            if (sim.TraitManager.HasElement(TraitNames.Friendly))
                weight += 5;
            if (sim.TraitManager.HasElement(TraitNames.CouchPotato))
                weight -= 5;
            if (sim.SimDescription.IsVampire && TimeUtils.IsNight)
                weight += 10;
            return weight;
        }

        public override int GetAmountOfPeopleForLot(Lot lot)
        {
            var baseline = Math.Min(20, Demographics.GetBaselineLotPopulation());
            var hotSpotStatus = lot.GetHotSpotStatus();
            if (Utils.SimFestAtLot(lot))
            {
                if (hotSpotStatus != HotSpotStatus.HotSpot)
                    baseline = (int)(baseline * 0.75);
                hotSpotStatus = HotSpotStatus.HotSpot;
            }
            if (TimeUtils.IsTherePrecipitation)
            {
                baseline = (int)(baseline * 0.5);
                if (hotSpotStatus == HotSpotStatus.DeadZone)
                    return 0;
            }
            if (hotSpotStatus == HotSpotStatus.HotSpot)
                return (int)(baseline * 2);
            if (hotSpotStatus == HotSpotStatus.DeadZone)
            {
                if (TimeUtils.IsMorning)
                    return 0;
                return (int)(baseline * 0.5);
            }
            if ((TimeUtils.IsEarlyMorning || TimeUtils.IsNight) && TimeUtils.IsTomorrowWeekendOrHoliday && !TimeUtils.IsMorning)
                return (int)(baseline * 1.25);
            if (TimeUtils.IsMorning)
                return 0;
            return baseline;
        }
    }
}
