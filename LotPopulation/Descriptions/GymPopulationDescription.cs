using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class GymPopulationDescription : LotPopulationDescription
    {
        public static LotPopulationDescription Singleton = new GymPopulationDescription();
        protected OutfitCategories outfitCategory = OutfitCategories.Athletic;

        public override int GetWeightForSim(Sim sim)
        {
            if (sim.SimDescription.ChildOrBelow)
                return 0;
            if (sim.TraitManager.HasElement(TraitNames.CouchPotato))
                return 0;
            if (sim.SimDescription.IsPregnant)
                return 0;
            var weight = 20;
            if (sim.TraitManager.HasElement(TraitNames.Athletic))
                weight += 20;
            if (sim.SimDescription.Elder)
                weight -= 20;
            if (sim.SimDescription.Adult)
                weight -= 5;
            return weight;
        }
        public override int GetAmountOfPeopleForLot(Lot lot)
        {
            var baseline = Math.Min(15, Demographics.GetBaselineLotPopulation());
            if (TimeUtils.IsTherePrecipitation)
                baseline = (int)(baseline * 0.5);
            if (TimeUtils.IsEarlyMorning)
                return 0;
            if (TimeUtils.IsMorning)
                return (int)(baseline * 0.5);
            if (TimeUtils.IsGettingLate)
                return (int)(baseline * 0.8);
            if (TimeUtils.IsNight)
                return 0;
            return baseline;
        }
        public override void SetupSim(Sim sim, Lot lot)
        {
            sim.Motives.SetValue(CommodityKind.Social, RandomUtil.GetFloat(-25f, 100f));
            sim.Motives.SetValue(CommodityKind.StayAtVenue, 100f);
            MakeSimDoRandomActionForCommodity(sim, CommodityKind.SkillAthletic, lot);
            sim.SwitchToOutfitWithoutSpin(outfitCategory);
        }

        public override void OnArrive(Sim sim, float x)
        {
            sim.SwitchToOutfitWithSpin(Sim.ClothesChangeReason.Force, outfitCategory, false, false);
        }
    }
}
