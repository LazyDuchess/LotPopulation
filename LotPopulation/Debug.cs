using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Core;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation
{
    public static class Debug
    {
        public static void Notify(string text, bool debugOnly = true)
        {
            if (debugOnly && !Main.kDebug)
                return;
        }

        public static void NotifyLot(string text, Lot lot, bool debugOnly = true)
        {
            Notify($"Lot: {lot.Name}\n{text}", debugOnly);
        }
    }
}
