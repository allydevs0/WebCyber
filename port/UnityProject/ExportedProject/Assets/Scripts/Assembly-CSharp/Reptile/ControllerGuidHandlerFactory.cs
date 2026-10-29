namespace Reptile
{
	public static class ControllerGuidHandlerFactory
	{
		public static IRewiredControllerGuidHandler CreateRewiredControllerGuidHandler(InputControllerGuidData inputControllerGuidData, APlatform platform)
		{
			return new SteamControllerGuidHandler(inputControllerGuidData, platform);
		}
	}
}
