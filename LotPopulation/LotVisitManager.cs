using LazyDuchess.LotPopulation.Descriptions;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation
{
    public class LotVisitManager : IDisposable
    {
        public static LotVisitManager Singleton = null;
        AlarmHandle _alarmHandle = AlarmHandle.kInvalidHandle;

        public LotVisitManager()
        {
            AddAlarm();
        }

        public static void Start()
        {
            Singleton = new LotVisitManager();
        }

        public static void Shutdown()
        {
            if (Singleton == null)
                return;
            Singleton.Dispose();
            Singleton = null;
        }

        void OnAlarm()
        {
            Simulate();
            AddAlarm();
        }

        void AddAlarm()
        {
            _alarmHandle = AlarmManager.Global.AddAlarm(RandomUtil.GetFloat(Main.kVisitLotIntervalMin, Main.kVisitLotIntervalMax), TimeUnit.Minutes, new AlarmTimerCallback(OnAlarm), "Lot Population Visit Alarm", AlarmType.NeverPersisted, null);
        }

        public static void QueueSimGoToLotInteraction(Sim sim, Lot lot, LotPopulationDescription lotDescription, Callback onStarted = null, Callback onCompleted = null, Callback onFailed = null)
        {
            QueueSimGoToLotInteraction(sim, lot, lotDescription.Outdoorsy, onStarted, onCompleted, onFailed);
        }

        public static void QueueSimGoToLotInteraction(Sim sim, Lot lot, bool outdoorsy = false, Callback onStarted = null, Callback onCompleted = null, Callback onFailed = null)
        {
            var interactionDefinition = VisitCommunityLot.Singleton;
            if (outdoorsy || lot.FindFrontDoor() == null)
                interactionDefinition = GoToCommunityLot.Singleton;
            var interactionInstance = interactionDefinition.CreateInstanceWithCallbacks(lot, sim, new InteractionPriority(InteractionPriorityLevel.Autonomous), true, true, onStarted, onCompleted, onFailed);
            sim.InteractionQueue.AddNext(interactionInstance);
        }

        void Simulate()
        {
            if (!GameStates.IsLiveState)
                return;
            var lots = LotPopulationManager.LotsInCameraDistance;
            foreach(var lot in lots)
            {
                if (LotPopulator.IsLotBeingPopulated(lot))
                    continue;
                if (!lot.IsOpenVenue())
                    continue;
                var description = LotPopulator.GetDescriptionForLot(lot);
                if (!description.Outdoorsy && LotManager.ActiveLot != lot)
                    continue;
                var simsInLotAlready = LotPopulator.GetSimsGoingToOrAtLot(lot);
                var targetSimAmount = description.GetAmountOfPeopleForLot(lot);
                if (targetSimAmount <= simsInLotAlready)
                    continue;
                var weightedCandidates = description.GetWeightedVisitCandidates(lot);
                if (weightedCandidates.Count <= 0)
                    continue;
                var simToVisit = RandomUtil.GetRandomObjectFromList(weightedCandidates);
                QueueSimGoToLotInteraction(simToVisit, lot, description, null, description.OnArrive, null);
                /*
                var interactionDefinition = VisitCommunityLot.Singleton;
                if (description.Outdoorsy || lot.FindFrontDoor() == null)
                    interactionDefinition = GoToCommunityLot.Singleton;
                var interactionInstance = interactionDefinition.CreateInstanceWithCallbacks(lot, simToVisit, new InteractionPriority(InteractionPriorityLevel.Autonomous), true, true, null, new Callback(description.OnArrive), null);
                simToVisit.InteractionQueue.AddNext(interactionInstance);*/
                Debug.NotifyLot($"{simToVisit.FullName} is visiting.", lot);
            }
        }

        public void Dispose()
        {
            if (_alarmHandle != AlarmHandle.kInvalidHandle)
                AlarmManager.Global.RemoveAlarm(_alarmHandle);
        }
    }
}
