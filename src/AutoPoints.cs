using System.IO;
using Pipliz;
using Chatting;
using Assets.ColonyPointUpgrades;
using Assets.ColonyPointUpgrades.Implementations;

namespace AutoPoints {

	public static class AutoPoints
	{
		public const string NAMESPACE = "AutoPoints";
		public const float CHECK_INTERVAL = 30.0f;
		public static string MOD_DIRECTORY;
		private static InterfaceManager interfaceClass = new InterfaceManager();

		public static void AssemblyLoaded(string path)
		{
			MOD_DIRECTORY = Path.GetDirectoryName(path);
			Log.Write("Loaded AutoPoints");
		}

		public static void WorldStarted()
		{
			ThreadManager.InvokeOnMainThread(delegate() {
				CheckColonies();
			}, CHECK_INTERVAL);
		}

		// check all colonies every 30 seconds
		public static void CheckColonies()
		{
			foreach (Colony colony in ServerManager.ColonyTracker.ColoniesByID.Values) {
				if (colony.ColonyPoints == colony.ColonyPointsCap) {
					ProcessColony(colony);
				}
			}

			// queue self again
			ThreadManager.InvokeOnMainThread(delegate() {
				CheckColonies();
			}, CHECK_INTERVAL);
		}

		// process one colony
		public static void ProcessColony(Colony colony)
		{
			foreach (Players.Player owner in colony.Owners) {
				if (owner.ConnectionState == Players.EConnectionState.Connected) {
					Chat.Send(owner, $"Colony {colony.Name} has reached point capacity {colony.ColonyPointsCap:N0}");
				}

				UpgradeKey KeyCapacity;
				IUpgrade UpgradeCapacity;
				ServerManager.UpgradeManager.TryGetKeyUpgrade("pipliz.colonypointcap", out KeyCapacity, out UpgradeCapacity);
				int lvl = colony.UpgradeState.GetUnlockedLevels(KeyCapacity);
				colony.UpgradeState.TryUnlock(colony, KeyCapacity, lvl);

				if (owner.ConnectionState == Players.EConnectionState.Connected) {
					Chat.Send(owner, $"Upgraded {colony.Name}. New limit is {colony.ColonyPointsCap:N0}");
				}
			}
		}

	} // class

} // namespace

