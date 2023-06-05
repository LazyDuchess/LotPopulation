using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Seasons;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class OutdoorPopulationDescription : LotPopulationDescription
    {
        public static LotPopulationDescription Singleton = new OutdoorPopulationDescription();
        public override float IndoorSpawnChance => 0f;
        public override bool Outdoorsy => true;
        public override float OutdoorSizePastBoundary => Main.kOutdoorLotSizePastBoundary;

        public override void SetupSim(Sim sim, Lot lot)
        {
            sim.Motives.SetValue(CommodityKind.Social, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.Fun, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.Hunger, RandomUtil.GetFloat(-10f, 100f));
            sim.Motives.SetValue(CommodityKind.StayAtVenue, 100f);
            if (RandomUtil.CoinFlip())
                MakeSimDoRandomActionForCommodity(sim, CommodityKind.Fun, lot);
            else
                MakeSimDoRandomActionForCommodity(sim, CommodityKind.Social, lot);
            if (SeasonsManager.Temperature <= Sim.kTemperatureToSwitchToOuterwear)
                sim.SwitchToOutfitWithoutSpin(OutfitCategories.Outerwear);
            else
                sim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday);
        }

        public override int GetAmountOfPeopleForLot(Lot lot)
        {
            if (TimeUtils.IsTherePrecipitation)
                return 0;
            var baseAmount = base.GetAmountOfPeopleForLot(lot);
            if (TimeUtils.Foggy)
                baseAmount = (int)(baseAmount * 0.5);
            if (SeasonsManager.Temperature <= 50f)
                baseAmount = (int)(baseAmount * 0.5);
            else
            {
                if (TimeUtils.IsSummer)
                    baseAmount = (int)(baseAmount * 1.5);
                if (TimeUtils.IsWinter)
                    baseAmount = (int)(baseAmount * 0.7);
            }
            return baseAmount;
        }

        public override int GetWeightForSim(Sim sim)
        {
            if (!sim.SimDescription.ChildOrAbove)
                return 0;

            var weight = 10;
            if (sim.SimDescription.Elder)
                weight = 5;

            if (sim.TraitManager.HasElement(TraitNames.Shy))
                return 1;
            if (sim.TraitManager.HasElement(TraitNames.Friendly))
                weight += 5;
            if (sim.TraitManager.HasElement(TraitNames.CouchPotato))
                weight -= 5;
            if (sim.TraitManager.HasElement(TraitNames.HatesOutdoors))
                weight -= 10;
            if (sim.TraitManager.HasElement(TraitNames.LovesTheOutdoors))
                weight += 10;
            return weight;
        }
    }
}
