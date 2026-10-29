namespace Reptile
{
	public static class UILocalizationUtility
	{
		public static string GetGameplayOnOffOption(IGameTextLocalizer localizer, bool value)
		{
			if (!value)
			{
				return localizer.GetUserInterfaceText("SETTINGS_OFF");
			}
			return localizer.GetUserInterfaceText("SETTINGS_ON");
		}
	}
}
