using Sims3.Gameplay.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class EmptyPopulationDescription : LotPopulationDescription
    {
        public static LotPopulationDescription Singleton = new EmptyPopulationDescription();
        public override int GetAmountOfPeopleForLot(Lot lot)
        {
            return 0;
        }
    }
}
