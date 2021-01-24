using System.IO;
using Pipliz;
using Chatting;
using Assets.ColonyPointUpgrades;
using Assets.ColonyPointUpgrades.Implementations;

namespace AutoPoints {

	public static class AutoPoints
	{
		public const string NAMESPACE = "AutoPoints";
		public const float CHECK_INTERVAL = 12.5f;
		public static string MOD_DIRECTORY;
		public static InterfaceManager interfaceClass = new InterfaceManager();
		private static UpgradeKey keyCapacity, keyEfficiency;
		private static ColonyPointCapacityUpgrade upgradeCapacity;
		private static ColonyPointMultiplierUpgrade upgradeEfficiency;

		public static void AssemblyLoaded(string path)
		{
			MOD_DIRECTORY = Path.GetDirectoryName(path);
			Log.Write("Loaded AutoPoints");
		}

		public static void WorldStarted()
		{
			IUpgrade iUpgradeCapacity, iUpgradeEfficiency;
			ServerManager.UpgradeManager.TryGetKeyUpgrade("pipliz.colonypointcap", out keyCapacity, out iUpgradeCapacity);
			ServerManager.UpgradeManager.TryGetKeyUpgrade("pipliz.pointmultiplier", out keyEfficiency, out iUpgradeEfficiency);
			upgradeCapacity = (ColonyPointCapacityUpgrade) iUpgradeCapacity;
			upgradeEfficiency = (ColonyPointMultiplierUpgrade) iUpgradeEfficiency;

			ThreadManager.InvokeOnMainThread(delegate() {
				CheckColonies();
			}, CHECK_INTERVAL);
		}

		// check all colonies every 30 seconds
		public static void CheckColonies()
		{
			foreach (Colony colony in ServerManager.ColonyTracker.ColoniesByID.Values) {
				if (colony.Banners.Length == 0 || colony.Owners.Length == 0) {
					continue;
				}

				CheckAndPerformEfficiencyUpgrade(colony);
				CheckAndPerformCapacityUpgrade(colony);
			}

			// queue self again
			ThreadManager.InvokeOnMainThread(delegate() {
				CheckColonies();
			}, CHECK_INTERVAL);
		}

		public static void CheckAndPerformEfficiencyUpgrade(Colony colony)
		{
			int lvlEfficiency = colony.UpgradeState.GetUnlockedLevels(keyEfficiency);
			if (lvlEfficiency < upgradeEfficiency.LevelCount) {
				long costEfficiency = upgradeEfficiency.GetUpgradeCost(lvlEfficiency);

				if (colony.ColonyPoints >= costEfficiency) {
					long? current = ColonyPointMultiplierUpgrade.GetCapacity(upgradeEfficiency.Levels, lvlEfficiency, 0);
					foreach (Players.Player owner in colony.Owners) {
						if (owner.ConnectionState == Players.EConnectionState.Connected) {
							Chat.Send(owner, $"Upgraded {colony.Name} points efficiency to {current + 100}%");
						}
					}
					colony.UpgradeState.TryUnlock(colony, keyEfficiency, lvlEfficiency);
				}
			}
		}

		public static void CheckAndPerformCapacityUpgrade(Colony colony)
		{
			int lvlCapacity = colony.UpgradeState.GetUnlockedLevels(keyCapacity);
			if (lvlCapacity < upgradeCapacity.LevelCount) {
				long costCapacity = upgradeCapacity.GetUpgradeCost(lvlCapacity);
				if (colony.ColonyPoints >= costCapacity) {
					long? current = upgradeCapacity.Levels[lvlCapacity].capacity;
					foreach (Players.Player owner in colony.Owners) {
						if (owner.ConnectionState == Players.EConnectionState.Connected) {
							Chat.Send(owner, $"Upgraded {colony.Name} max point capacity to {current:N0}");
						}
					}
					colony.UpgradeState.TryUnlock(colony, keyCapacity, lvlCapacity);
				}
			}
		}

	} // class

} // namespace

