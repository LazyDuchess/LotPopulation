using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Seasons;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class BeachPopulationDescription : OutdoorPopulationDescription
    {
        public static LotPopulationDescription Singleton = new BeachPopulationDescription();
        public override float OutdoorSizePastBoundary => Main.kBeachLotSizePastBoundary;
        public override int GetWeightForSim(Sim sim)
        {
            if (sim.SimDescription.IsPregnant)
                return 0;
            return base.GetWeightForSim(sim);
        }
        public override int GetAmountOfPeopleForLot(Lot lot)
        {
            var baseline = Math.Min(20, Demographics.GetBaselineLotPopulation());
            if (SeasonsManager.Temperature <= 59)
                baseline = (int)(baseline * 0.5);
            if (SeasonsManager.Temperature <= 50)
                return 0;
            if (TimeUtils.Foggy || TimeUtils.IsTherePrecipitation || TimeUtils.SnowOnGround)
                return 0;
            if (TimeUtils.IsTodayWeekendOrHoliday)
            {
                if (TimeUtils.IsEarlyMorning)
                    return 0;
                if (TimeUtils.IsMorning)
                    return Math.Min(5, (int)(baseline * 0.3f));
                if (TimeUtils.IsGettingLate)
                    return (int)(baseline * 0.8f);
                if (TimeUtils.IsNight)
                {
                    if (TimeUtils.IsTomorrowWeekendOrHoliday)
                        return Math.Min(5, (int)(baseline * 0.3f));
                    else
                        return 0;
                }
                if (TimeUtils.IsSummer)
                    return (int)(baseline * 1.5f);
                return (int)(baseline * 1.2f);
            }
            else
            {
                if (TimeUtils.IsEarlyMorning)
                    return 0;
                if (TimeUtils.IsMorning)
                    return 0;
                if (TimeUtils.IsGettingLate)
                    return (int)(baseline * 0.8f);
                if (TimeUtils.IsNight)
                {
                    if (TimeUtils.IsTomorrowWeekendOrHoliday)
                        return Math.Min(5, (int)(baseline * 0.3f));
                    else
                        return 0;
                }
                if (TimeUtils.IsSummer)
                    return (int)(baseline * 1.2f);
                return baseline;
            }
        }

        public override void SetupSim(Sim sim, Lot lot)
        {
            sim.Motives.SetValue(CommodityKind.Social, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.Fun, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.StayAtVenue, 100f);
            if (RandomUtil.CoinFlip())
                MakeSimDoRandomActionForCommodity(sim, CommodityKind.Fun, lot);
            else
                MakeSimDoRandomActionForCommodity(sim, CommodityKind.Social, lot);
            sim.SwitchToOutfitWithoutSpin(OutfitCategories.Swimwear);
        }

        public override void OnArrive(Sim sim, float x)
        {
            sim.SwitchToOutfitWithSpin(Sim.ClothesChangeReason.Force, OutfitCategories.Swimwear, false, false);
        }
    }
}
