using LazyDuchess.LotPopulation.Descriptions;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Seasons;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.SimIFace.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation
{
    public class LotWalkbyManager : IDisposable
    {
        public static LotWalkbyManager Singleton = null;
        AlarmHandle _alarmHandle = AlarmHandle.kInvalidHandle;

        [PersistableStatic]
        static Dictionary<Sim, bool> simsWalkedByRecently = new Dictionary<Sim, bool>();

        static void RemoveSimCooldown(Sim sim)
        {
            if (IsSimInCooldown(sim))
                simsWalkedByRecently.Remove(sim);
        }

        static void PutSimInCooldown(Sim sim)
        {
            simsWalkedByRecently[sim] = true;
            AlarmManager.Global.AddAlarm(Main.kWalkByCooldown, TimeUnit.Minutes, () => { RemoveSimCooldown(sim); }, "WalkBy Cooldown Remove", AlarmType.AlwaysPersisted, null);
        }

        static bool IsSimInCooldown(Sim sim)
        {
            if (simsWalkedByRecently.TryGetValue(sim, out bool _))
                return true;
            return false;
        }

        public LotWalkbyManager()
        {
            AddAlarm();
        }

        public static void Start()
        {
            Singleton = new LotWalkbyManager();
        }

        public static void Shutdown()
        {
            if (Singleton == null)
                return;
            Singleton.Dispose();
            Singleton = null;
        }

        void AddAlarm()
        {
            _alarmHandle = AlarmManager.Global.AddAlarm(GetWalkByInterval(), TimeUnit.Minutes, new AlarmTimerCallback(OnAlarm), "WalkBy Alarm", AlarmType.NeverPersisted, null);
        }

        void OnAlarm()
        {
            if (!Utils.IsCameraInTheSky())
            {
                var chance = GetWalkByChance();
                if (RandomUtil.RandomChance(chance))
                    SpawnRandomWalkByAsync();
            }
            AddAlarm();
        }

        static List<Vector3> GetSidewalksForLot(Lot lot)
        {
            var offsetFromCenter = 2f;
            var corners = lot.Corners.mPoints;
            var sidewalks = new List<Vector3>();
            var midPoint = lot.GetCenterPosition();
            midPoint.y = 0f;
            foreach(var corner in corners)
            {
                var newCorner = corner;
                newCorner.y = 0f;
                var heading = (corner - midPoint).Normalize();
                var sideWalkCorner = World.SnapToFloor(corner + (heading * offsetFromCenter));
                if (World.GetTerrainSurfaceType(sideWalkCorner) == TerrainTypes.Road01_Sidewalk)
                    sidewalks.Add(sideWalkCorner);
            }
            return sidewalks;
        }

        static float GetWalkByChance()
        {
            var baseline = Demographics.GetBaselineLotPopulation(out int totalSims);
            if (totalSims <= Main.kWalkbyTotalSimsMinimum)
                return 0f;
            baseline = (int)(baseline * 5f);
            if (TimeUtils.IsTherePrecipitation)
            {
                    baseline = (int)(baseline * 0.5f);
            }
            if (SeasonsManager.CurrentSnowLevel >= SnowLevel.Medium)
                baseline = (int)(baseline * 0.5f);
            if (SeasonsManager.Temperature <= 50f)
                baseline = (int)(baseline * 0.75f);
            if (TimeUtils.IsEarlyMorning)
                baseline = (int)(baseline * 0.25f);
            if (TimeUtils.IsNight)
                baseline = (int)(baseline * 0.6f);
            return Math.Min(100f,baseline);
        }

        static float GetWalkByInterval()
        {
            if (TimeUtils.IsEarlyMorning)
                return RandomUtil.GetFloat(10f, 20f);
            if (TimeUtils.IsNight)
                return RandomUtil.GetFloat(8f, 12f);
            return RandomUtil.GetFloat(6f, 9f);
        }

        static List<Sim> GetWalkByCandidates()
        {
            var candidates = new List<Sim>();
            foreach (var sim in LotManager.Actors)
            {
                if (Main.kOnlyTeleportSimsDoingNothing)
                {
                    if (sim.CurrentInteraction != null)
                        continue;
                }
                if (sim.SimDescription.ChildOrBelow)
                    continue;
                if (sim.IsSocializing)
                    continue;
                if (sim.IsGhostOrHasGhostBuff)
                    continue;
                if (!sim.IsHuman)
                    continue;
                if (sim.IsAtWork)
                    continue;
                if (sim.IsDying())
                    continue;
                if (sim.IsPerformingAService)
                    continue;
                if (!sim.IsStandingIdle)
                    continue;
                if (sim.IsSelectable)
                    continue;
                if (sim.IsSleeping)
                    continue;
                if (LotPopulationDescription.IsSimInLotWithASelectableSim(sim))
                    continue;
                if (sim.GetSituationOfType<Situation>() != null)
                    continue;
                if (sim.SimDescription.HasActiveRole)
                    continue;
                if (LotPopulationDescription.IsSimOpportunityTarget(sim))
                    continue;
                if (LotPopulationDescription.IsSimInCurfew(sim))
                    continue;
                if (sim.RabbitHoleCurrent != null)
                    continue;
                if (sim.MoodManager.IsInNegativeMood)
                    continue;
                if (IsSimInCooldown(sim))
                    continue;
                if (Utils.IsSimPerforming(sim))
                    continue;
                if (sim.Parent != null)
                    continue;
                if (sim.SimDescription.IsVampire && !SimClock.IsNightTime())
                    continue;
                if (!Utils.IsCameraInTheSky())
                {
                    if (sim.IsOutside)
                    {
                        if (World.IsObjectOnScreen(sim))
                            continue;
                    }
                    else
                    {
                        if (sim.LotCurrent != null && sim.LotCurrent != LotManager.sWorldLot)
                        {
                            if (Utils.IsLotOnScreen(sim.LotCurrent) && LotManager.ActiveLot == sim.LotCurrent)
                                continue;
                        }
                    }
                }
                if (Household.ActiveHouseholdLot != null)
                {
                    if (sim.LotCurrent == Household.ActiveHouseholdLot || sim.LotHome == Household.ActiveHouseholdLot)
                        continue;
                }
                if (Household.ActiveHousehold != null)
                {
                    if (sim.Household == Household.ActiveHousehold)
                        continue;
                }
                candidates.Add(sim);
            }
            return candidates;
        }

        static Vector3 FindSpotGoingThrough(Vector3 from, Vector3 reference, List<Vector3> posCandidates)
        {
            if (posCandidates.Count <= 0)
                return Vector3.Invalid;
            reference.y = 0f;
            from.y = 0f;
            var headingToReference = (reference - from).Normalize();
            var candidates = new List<Vector3>();
            foreach (var pos in posCandidates)
            {
                var referencePos = pos;
                referencePos.y = 0f;
                var headingToPos = (referencePos - reference).Normalize();
                var lotDot = (headingToPos * headingToReference);
                if (lotDot > 0f)
                    candidates.Add(pos);
            }
            if (candidates.Count <= 0)
                return Vector3.Invalid;
            return RandomUtil.GetRandomObjectFromList(candidates);
        }

        static Lot FindLotGoingThrough(Vector3 from, Vector3 reference)
        {
            reference.y = 0f;
            from.y = 0f;
            var headingToReference = (reference - from).Normalize();
            var commLots = LotManager.GetAllCommunityLots();
            var candidates = new List<Lot>();
            foreach(var lot in commLots)
            {
                var dist = lot.GetDistanceToLotBoundary(World.SnapToFloor(reference));
                if (dist < Main.kWalkBySideWalkDistance)
                    continue;
                var lotCenter = lot.GetCenterPosition();
                lotCenter.y = 0f;
                var headingToLot = (lotCenter - reference).Normalize();
                var lotDot = (headingToLot * headingToReference);
                if (lotDot > 0f)
                    candidates.Add(lot);
            }
            if (candidates.Count <= 0)
                return null;
            return RandomUtil.GetRandomObjectFromList(candidates);
        }

        public static void SpawnRandomWalkByAsync()
        {
            Simulator.AddObject(new OneShotFunctionTask(SpawnRandomWalkBy));
        }

        public static void SpawnRandomWalkBy()
        {
            try
            {
                var sideWalks = GetIdealWalkByPoints();

                var offScreenIdealSideWalks = sideWalks.offscreenIdealSideWalks;
                var offScreenFarSideWalks = sideWalks.offscreenFarSideWalks;

                var allFarSideWalks = sideWalks.allFarSideWalks;
                var allIdealSideWalks = sideWalks.allIdealSideWalks;

                if (offScreenIdealSideWalks.Count <= 0)
                    offScreenIdealSideWalks = offScreenFarSideWalks;
                if (offScreenIdealSideWalks.Count <= 0)
                {
                    Debug.Notify("Couldn't find sidewalks.");
                    return;
                }
                var sideWalk = RandomUtil.GetRandomObjectFromList(offScreenIdealSideWalks);
                var candidates = GetWalkByCandidates();
                if (candidates.Count <= 0)
                {
                    Debug.Notify("Couldn't find Sims.");
                    return;
                }
                offScreenIdealSideWalks.Remove(sideWalk);
                offScreenFarSideWalks.Remove(sideWalk);
                allIdealSideWalks.Remove(sideWalk);
                allFarSideWalks.Remove(sideWalk);
                var sim = RandomUtil.GetRandomObjectFromList(candidates);
                var cameraTarget = CameraController.GetTarget();
                var targetSpot = FindSpotGoingThrough(sideWalk, cameraTarget, offScreenFarSideWalks);
                if (targetSpot == Vector3.Invalid)
                {
                    targetSpot = FindSpotGoingThrough(sideWalk, cameraTarget, allFarSideWalks);
                    if (targetSpot == Vector3.Invalid)
                    {
                        targetSpot = FindSpotGoingThrough(sideWalk, cameraTarget, offScreenIdealSideWalks);
                        if (targetSpot == Vector3.Invalid)
                        {
                            targetSpot = FindSpotGoingThrough(sideWalk, cameraTarget, allIdealSideWalks);
                        }
                    }
                }
                if (targetSpot == Vector3.Invalid)
                {
                    Debug.Notify("Couldn't find Spots across.");
                    return;
                }
                sim.SetPosition(sideWalk);
                sim.PopPosture();
                Sim.RemoveGoHomeInteractions(sim, false);
                sim.SetObjectToReset();
                sim.BuffManager.RemoveElement(BuffNames.Singed);
                sim.BuffManager.RemoveElement(BuffNames.SingedElectricity);
                if (Simulator.CheckYieldingContext(false))
                    Simulator.Sleep(0);
                //sim.RequestWalkStyle(Sim.WalkStyle.ForceWalk);
                //targetLot.GetClosestCornerToPoint(sim.Position, out Vector3 closestCorner);
                var interactionDefinition = Terrain.GoHere.GetSingleton(sim, targetSpot);
                var interactionInstance = interactionDefinition.CreateInstanceWithCallbacks(Terrain.Singleton, sim, new InteractionPriority(InteractionPriorityLevel.Autonomous), false, false, OnStartWalkBy, OnEndWalkBy, OnEndWalkBy);
                (interactionInstance as Terrain.GoHere).SetDestination(targetSpot, true);
                sim.InteractionQueue.AddNext(interactionInstance);
                if (SeasonsManager.Temperature <= Sim.kTemperatureToSwitchToOuterwear)
                    sim.SwitchToOutfitWithoutSpin(OutfitCategories.Outerwear);
                else
                    sim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday);
                PutSimInCooldown(sim);
                Debug.Notify($"Spawned walkby {sim.FullName}.");
            }
            catch(Exception e)
            {
                Debug.Notify(e.ToString());
            }
        }

        static void OnStartWalkBy(Sim sim, float x)
        {
            sim.RequestWalkStyle(Sim.WalkStyle.ForceWalk);
        }

        static void OnEndWalkBy(Sim sim, float x)
        {
            sim.UnrequestWalkStyle(Sim.WalkStyle.ForceWalk);
            sim.Autonomy.TryFindBestAction();
        }

        public struct SideWalkResult
        {
            public List<Vector3> offscreenIdealSideWalks;
            public List<Vector3> offscreenFarSideWalks;
            public List<Vector3> allFarSideWalks;
            public List<Vector3> allIdealSideWalks;
        }

        public static SideWalkResult GetIdealWalkByPoints()
        {
            var farSideWalks = new List<Vector3>();
            var idealSideWalks = new List<Vector3>();

            var allFarSideWalks = new List<Vector3>();
            var allIdealSideWalks = new List<Vector3>();

            var cameraPosition = CameraController.GetPosition();
            foreach(var lot in LotManager.Lots)
            {
                if (lot.GetDistanceToLotBoundary(cameraPosition) <= Main.kWalkBySideWalkDistance)
                {
                    var sidewalksForLot = GetSidewalksForLot(lot);
                    foreach(var sidewalk in sidewalksForLot)
                    {
                        var sideWalkDistance = (sidewalk - cameraPosition).Length();
                        if (sideWalkDistance <= Main.kWalkBySideWalkDistance)
                        {
                            if (sideWalkDistance <= Main.kWalkBySideWalkDistanceIdeal)
                            {
                                if (Utils.IsCameraInTheSky() || !Utils.IsPointOnScreen(sidewalk))
                                    idealSideWalks.Add(sidewalk);
                                allIdealSideWalks.Add(sidewalk);
                            }
                            else
                            {
                                if (Utils.IsCameraInTheSky() || !Utils.IsPointOnScreen(sidewalk))
                                    farSideWalks.Add(sidewalk);
                                allFarSideWalks.Add(sidewalk);
                            }
                        }
                    }
                }
            }
            return new SideWalkResult()
            {
                offscreenIdealSideWalks = idealSideWalks,
                offscreenFarSideWalks = farSideWalks,
                allIdealSideWalks = allIdealSideWalks,
                allFarSideWalks = allFarSideWalks
            };
        }

        public void Dispose()
        {
            if (_alarmHandle != AlarmHandle.kInvalidHandle)
                AlarmManager.Global.RemoveAlarm(_alarmHandle);
        }
    }
}
