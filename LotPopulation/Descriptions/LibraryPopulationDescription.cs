using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class LibraryPopulationDescription : LotPopulationDescription
    {
        public static LotPopulationDescription Singleton = new LibraryPopulationDescription();

        public override int GetWeightForSim(Sim sim)
        {
            var weight = base.GetWeightForSim(sim);
            if (weight == 0)
                return 0;
            if (sim.TraitManager.HasElement(TraitNames.BookWorm))
                weight += 20;
            return weight;
        }
        public override void SetupSim(Sim sim, Lot lot)
        {
            sim.Motives.SetValue(CommodityKind.Social, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.Fun, RandomUtil.GetFloat(-40f, 25f));
            sim.Motives.SetValue(CommodityKind.StayAtVenue, 100f);
            MakeSimDoRandomActionForCommodity(sim, CommodityKind.TraitBookworm, lot);
            sim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday);
        }
    }
}
