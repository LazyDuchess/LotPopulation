using Sims3.Gameplay.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation
{
    public static class Demographics
    {
        public static int GetBaselineLotPopulation()
        {
            return GetBaselineLotPopulation(out int _);
        }
        public static int GetBaselineLotPopulation(out int totalSims)
        {
            var allSims = LotManager.Actors;
            var simCount = 0;
            foreach(var sim in allSims)
            {
                if (sim.IsPerformingAService)
                    continue;
                if (sim.IsGhostOrHasGhostBuff)
                    continue;
                if (!sim.IsHuman)
                    continue;
                if (sim.IsSelectable)
                    continue;
                if (!sim.SimDescription.TeenOrAbove)
                    continue;
                if (sim.SimDescription.HasActiveRole)
                    continue;
                simCount++;
            }
            totalSims = simCount;
            simCount = (int)(simCount * Main.kDemographicsMultiply);
            if (simCount <= 1)
                simCount = 1;
            if (simCount >= Main.kDemographicsMax)
                simCount = Main.kDemographicsMax;
            return simCount;
        }
    }
}
