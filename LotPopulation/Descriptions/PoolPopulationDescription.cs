using Sims3.Gameplay.Core;
using Sims3.Gameplay.Seasons;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class PoolPopulationDescription : GymPopulationDescription
    {
        public static LotPopulationDescription Singleton = new PoolPopulationDescription();
        public override float IndoorSpawnChance => 0f;
        public override bool Outdoorsy => true;
        public override float OutdoorSizePastBoundary => Main.kPoolLotSizePastBoundary;
        public override int GetAmountOfPeopleForLot(Lot lot)
        {
            if (SeasonsManager.Temperature <= 50f)
                return 0;
            if (TimeUtils.SnowOnGround || TimeUtils.Foggy)
                return 0;
            return OutdoorPopulationDescription.Singleton.GetAmountOfPeopleForLot(lot);
        }
        public PoolPopulationDescription()
        {
            outfitCategory = OutfitCategories.Swimwear;
        }
    }
}
