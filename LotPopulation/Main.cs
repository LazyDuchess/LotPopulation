using System;
using System.Collections.Generic;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.MapTags;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.Gameplay.Opportunities;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;

//Template Created by Battery

namespace LazyDuchess.LotPopulation
{
	public class Main
	{
		[Tunable] static bool init;

		// Enable lot population.
		[Tunable] public static bool kPopulateLots = true;

		[Tunable] public static bool kPopulateLotsOnLookInside = true;
		[Tunable] public static bool kPopulateOutdoorLotsByCameraDistance = true;
		// If higher than 0, will populate outdoorsy lots based on camera distance on ground.
		[Tunable] public static float kOutdoorLotCameraDistanceMinimum = 80f;
		// If higher than 0, will populate outdoorsy lots based on camera target distance while in map view.
		[Tunable] public static float kOutdoorLotMapViewDistanceMinimum = 0f;

		[Tunable] public static bool kDebug = false;

		[Tunable] public static bool kOnlyTeleportSimsDoingNothing = false;

		[Tunable] public static bool kWaitForPopulationToFinish = true;

		// Hours. How long low priority for teleported Sims lasts.
		[Tunable] public static float kSimLowPriorityDuration = 1f;

		// Minutes. Apply a cooldown to lots you're looking at so that new sims don't spawn immediately upon leaving view.
		[Tunable] public static bool kLookingAtCooldownEnabled = true;
		[Tunable] public static float kLookingAtLotCooldown = 30f;

		// Enable Sims queuing Go To on community lots in view to fill population quota.
		[Tunable] public static bool kVisitLots = true;
		// Minutes. Interval of Sims visiting lots you're looking at.
		[Tunable] public static float kVisitLotIntervalMin = 5f;
		[Tunable] public static float kVisitLotIntervalMax = 10f;

		[Tunable] public static float kOutdoorLotSizePastBoundary = 10f;
		[Tunable] public static float kBeachLotSizePastBoundary = 20f;
		[Tunable] public static float kPoolLotSizePastBoundary = 0f;

		[Tunable] public static bool kUseReferenceSimFromLot = false;

		[Tunable] public static bool kOnlyPopulateInLiveMode = false;

		// Enable WalkBys.
		[Tunable] public static bool kWalkBys = true;
		[Tunable] public static float kWalkBySideWalkDistance = 120f;
		[Tunable] public static float kWalkBySideWalkDistanceIdeal = 50f;
		// Minutes. Cooldown for walkbys.
		[Tunable] public static float kWalkByCooldown = 30f;
		// How much to multiply the amount of Sims in the world for to get the base population value. 0.1 yields 5 for most base TS3 worlds, spawning an average of 5 Sims per lot, give or take given several factors.
		[Tunable] public static float kDemographicsMultiply = 0.2f;
		// Maximum population value calculated from demographics.
		[Tunable] public static int kDemographicsMax = 20;
		// Minimum amount of Sims in world for walkbys to show up.
		[Tunable] public static int kWalkbyTotalSimsMinimum = 20;

		static Main()
		{
			World.sOnStartupAppEventHandler += OnStartup;
			World.sOnWorldLoadFinishedEventHandler += OnWorldLoad;
			World.sOnWorldQuitEventHandler += OnWorldQuit;
		}

		static void OnWorldLoad(object sender, EventArgs e)
        {
			if (kPopulateLots)
				LotPopulationManager.Start();
			if (kVisitLots)
				LotVisitManager.Start();
			if (kWalkBys)
				LotWalkbyManager.Start();
        }

		static void OnWorldQuit(object sender, EventArgs e)
        {
			LotPopulationManager.Shutdown();
			LotVisitManager.Shutdown();
			LotWalkbyManager.Shutdown();
        }

		static void OnStartup(object sender, EventArgs e)
		{
			Commands.sGameCommands.Register("lotcorners", "Get the corners of current lot.", Commands.CommandType.General, new CommandHandler(CommandGetLotCorners));
			Commands.sGameCommands.Register("lotonscreen", "Get if current lot is on screen.", Commands.CommandType.General, new CommandHandler(CommandGetLotOnScreen));
			Commands.sGameCommands.Register("lotcenter", "Get the screen position of the current lot's center.", Commands.CommandType.General, new CommandHandler(CommandGetLotCenterOnScreen));
			Commands.sGameCommands.Register("lotpopulate", "Populates the lot that the active Sim is at.", Commands.CommandType.General, new CommandHandler(CommandLotPopulate));
			Commands.sGameCommands.Register("getroomid", "Gets the room ID of the active Sim.", Commands.CommandType.General, new CommandHandler(CommandGetRoomId));
			Commands.sGameCommands.Register("getactivelot", "Gets the active lot.", Commands.CommandType.General, new CommandHandler(CommandGetActiveLot));
			Commands.sGameCommands.Register("lotinfo", "Gets info for active lot population.", Commands.CommandType.General, new CommandHandler(CommandGetActiveLotInfo));
			Commands.sGameCommands.Register("demographics", "Gets demographics for lot population.", Commands.CommandType.General, new CommandHandler(CommandGetDemographics));
			Commands.sGameCommands.Register("cameraheight", "Get camera height off the ground.", Commands.CommandType.General, new CommandHandler(CommandGetCameraOffGroundHeight));
			Commands.sGameCommands.Register("cameratargetsurface", "Get Surface Type of camera target.", Commands.CommandType.General, new CommandHandler(CommandGetCameraTargetSurfaceType));
			Commands.sGameCommands.Register("getsidewalks", "Get Near Walkby Spawn Points.", Commands.CommandType.General, new CommandHandler(CommandGetSideWalks));
			Commands.sGameCommands.Register("walkby", "Spawns a walkby.", Commands.CommandType.General, new CommandHandler(CommandSpawnWalkBy));
		}

		static void MakeDebugTruck(Vector3 position)
        {
			var debugTruck = GlobalFunctions.CreateObjectOutOfWorld("carTruckIceCream", ProductVersion.EP5, "Sims3.Gameplay.Objects.Vehicles.IceCreamTruck", null) as IceCreamTruck;
			debugTruck.PlaceAt(position, Vector3.UnitZ, null);
			var mapTagManager = MapTagManager.ActiveMapTagManager;
			mapTagManager.AddTag(debugTruck);
			AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, new AlarmTimerCallback(() =>
			{
				mapTagManager.RemoveTag(debugTruck);
				debugTruck.Destroy();
			}), "Debug Truck Removal", AlarmType.AlwaysPersisted, null);
		}

		static int CommandSpawnWalkBy(object[] parameters)
        {
			LotWalkbyManager.SpawnRandomWalkByAsync();
			return 0;
        }

		static int CommandGetSideWalks(object[] parameters)
        {
			var sideWalks = LotWalkbyManager.GetIdealWalkByPoints();
			var allSideWalks = sideWalks.allFarSideWalks;
			allSideWalks.AddRange(sideWalks.allIdealSideWalks);
			foreach(var sidewalk in allSideWalks)
            {
				MakeDebugTruck(sidewalk);
            }
			return 0;
		}

		static int CommandGetCameraTargetSurfaceType(object[] parameters)
        {
			var cameraTargetPosition = CameraController.GetTarget();
			var surface = World.GetTerrainSurfaceType(cameraTargetPosition);
			StyledNotification.Show(new StyledNotification.Format(surface.ToString(), StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
        }

		static int CommandGetCameraOffGroundHeight(object[] parameters)
        {
			var cameraPosition = CameraController.GetPosition();
			var cameraFloorPosition = World.SnapToFloor(cameraPosition);
			var heightDifference = cameraPosition.y - cameraFloorPosition.y;
			StyledNotification.Show(new StyledNotification.Format(heightDifference.ToString(), StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
        }

		static int CommandGetDemographics(object[] parameters)
		{
			var baseline = Demographics.GetBaselineLotPopulation(out int totalSims);
			StyledNotification.Show(new StyledNotification.Format(
				$"Baseline: {baseline}\n" +
				$"Total Sims: {totalSims}"
			, StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
		}

		static int CommandGetActiveLotInfo(object[] parameters)
		{
			var lot = LotManager.ActiveLot;
			if (lot == null)
				return -1;
			var description = LotPopulator.GetDescriptionForLot(lot);
			if (description == null)
				return -1;
			var onCoolDown = LotPopulationManager.IsLotOnCooldown(lot);
			StyledNotification.Show(new StyledNotification.Format(
				$"Info for: {LotManager.ActiveLot.Name}\n" +
				$"Lot Population Description: {description.GetType().FullName}\n" +
				$"Target Amount of People: {description.GetAmountOfPeopleForLot(lot)}\n" +
				$"Sims in lot or Visiting: {LotPopulator.GetSimsGoingToOrAtLot(lot)}\n" +
				$"On Cooldown: {onCoolDown}"
			, StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
		}

		static int CommandGetActiveLot(object[] parameters)
        {
			StyledNotification.Show(new StyledNotification.Format(LotManager.ActiveLot.Name, StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
		}

		static int CommandLotPopulate(object[] parameters)
		{
			LotPopulator.PopulateLotAsync(PlumbBob.SelectedActor.LotCurrent, LotPopulator.Settings.Default);
			return 0;
		}

		static int CommandGetRoomId(object[] parameters)
        {
			StyledNotification.Show(new StyledNotification.Format($"RoomID: {PlumbBob.SelectedActor.RoomId}, Outside: {PlumbBob.SelectedActor.IsOutside}", StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
		}

		static int CommandGetLotCenterOnScreen(object[] parameters)
        {
			if (Household.ActiveHouseholdLot == null)
				return -1;
			StyledNotification.Show(new StyledNotification.Format(Utils.GetNormalizedScreenPosition(Household.ActiveHouseholdLot.Position).ToString(), StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
		}

		static int CommandGetLotOnScreen(object[] parameters)
		{
			if (Household.ActiveHouseholdLot == null)
				return -1;
			StyledNotification.Show(new StyledNotification.Format(Utils.IsLotOnScreen(Household.ActiveHouseholdLot).ToString(), StyledNotification.NotificationStyle.kGameMessagePositive));
			return 0;
		}

		static int CommandGetLotCorners(object[] parameters)
        {
			if (Household.ActiveHouseholdLot == null)
				return -1;
			var bbox = Utils.GetLotBoundingBox(Household.ActiveHouseholdLot);
			foreach (var corner in bbox)
			{
				var debugTruck = GlobalFunctions.CreateObjectOutOfWorld("carTruckIceCream", ProductVersion.EP5, "Sims3.Gameplay.Objects.Vehicles.IceCreamTruck", null) as IceCreamTruck;
				debugTruck.PlaceAt(corner, Vector3.UnitZ, null);
				var mapTagManager = MapTagManager.ActiveMapTagManager;
				mapTagManager.AddTag(debugTruck);
				AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, new AlarmTimerCallback(() =>
				{
					mapTagManager.RemoveTag(debugTruck);
					debugTruck.Destroy();
				}), "Debug Truck Removal", AlarmType.AlwaysPersisted, null);
			}
			return 0;
        }

		public static void Initialize()
		{

		}
	}
}