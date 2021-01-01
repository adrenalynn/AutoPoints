using ModLoaderInterfaces;

namespace AutoPoints {

	public class InterfaceManager:
		IOnAssemblyLoaded,
		IAfterWorldLoad
	{
		public void OnAssemblyLoaded(string path)
		{
			AutoPoints.AssemblyLoaded(path);
		}

		public void AfterWorldLoad()
		{
			AutoPoints.WorldStarted();
		}

	}

} // namespace

