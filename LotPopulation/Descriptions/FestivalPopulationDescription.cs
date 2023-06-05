using Sims3.Gameplay.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation.Descriptions
{
    public class FestivalPopulationDescription : OutdoorPopulationDescription
    {
        public static LotPopulationDescription Singleton = new FestivalPopulationDescription();
        public override int GetAmountOfPeopleForLot(Lot lot)
        {
            if (TimeUtils.OnHoliday)
            {
                var baseline = Demographics.GetBaselineLotPopulation();
                if (TimeUtils.IsTherePrecipitation || TimeUtils.Foggy)
                    baseline = (int)(baseline * 0.8);
                if (TimeUtils.IsMorning || TimeUtils.IsNight)
                    baseline = (int)(baseline * 0.75);
                if (TimeUtils.IsEarlyMorning)
                    return 0;
                return baseline;
            }
            return base.GetAmountOfPeopleForLot(lot);
        }
    }
}
