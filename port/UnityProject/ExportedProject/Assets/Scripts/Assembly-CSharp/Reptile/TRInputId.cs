using Rewired.Dev;

namespace Reptile
{
	public static class TRInputId
	{
		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "move horizontal")]
		public const int moveX = 5;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "move vertical")]
		public const int moveY = 6;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "jump")]
		public const int jump = 7;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "slide")]
		public const int slide = 8;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "Spray Interact")]
		public const int spray = 10;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "switchStyle")]
		public const int switchStyle = 11;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "trick 2")]
		public const int trick2 = 12;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "camera horizontal")]
		public const int cameraX = 13;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "camera vertical")]
		public const int cameraY = 14;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "trick 1")]
		public const int trick1 = 15;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "walk")]
		public const int walk = 16;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "dance")]
		public const int dance = 17;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "boost")]
		public const int boost = 18;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "LocalizationUpdate")]
		public const int LocalizationUpdate = 49;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "pause")]
		public const int pause = 4;

		[ActionIdFieldInfo(categoryName = "Character Actions", friendlyName = "trick 3")]
		public const int trick3 = 65;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuX")]
		public const int menuX = 0;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuY")]
		public const int menuY = 1;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuConfirm")]
		public const int menuConfirm = 2;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuCancel")]
		public const int menuCancel = 3;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuCycleLeft")]
		public const int menuCycleLeft = 45;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuCycleRight")]
		public const int menuCycleRight = 47;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuReset")]
		public const int menuReset = 48;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "For skipping dialogue")]
		public const int menuSkip = 64;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuMouseConfirm")]
		public const int menuMouseConfirm = 66;

		[ActionIdFieldInfo(categoryName = "Menu Controls", friendlyName = "menuZoom")]
		public const int menuZoom = 67;

		[ActionIdFieldInfo(categoryName = "Phone Controls", friendlyName = "Phone Select")]
		public const int PhoneSelect = 19;

		[ActionIdFieldInfo(categoryName = "Phone Controls", friendlyName = "Phone Back")]
		public const int PhoneReturn = 20;

		[ActionIdFieldInfo(categoryName = "Phone Controls", friendlyName = "Phone Move Up")]
		public const int PhoneMoveUp = 21;

		[ActionIdFieldInfo(categoryName = "Phone Controls", friendlyName = "Phone Move Right")]
		public const int PhoneMoveRight = 29;

		[ActionIdFieldInfo(categoryName = "Phone Controls", friendlyName = "Phone Move Down")]
		public const int PhoneMoveDown = 56;

		[ActionIdFieldInfo(categoryName = "Phone Controls", friendlyName = "Phone Move Left")]
		public const int PhoneMoveLeft = 57;

		[ActionIdFieldInfo(categoryName = "Phone Controls", friendlyName = "Phone Remove Element")]
		public const int PhoneRemove = 59;

		[ActionIdFieldInfo(categoryName = "For controlling debug modules", friendlyName = "Toggle Debug menu")]
		public const int ToggleDebugMenu = 40;

		[ActionIdFieldInfo(categoryName = "For controlling debug modules", friendlyName = "SlowMo")]
		public const int SlowMo = 41;

		[ActionIdFieldInfo(categoryName = "For controlling debug modules", friendlyName = "Scroll the log")]
		public const int LogScroll = 42;

		[ActionIdFieldInfo(categoryName = "For controlling debug modules", friendlyName = "Scroll to end of log")]
		public const int LogScrollToEnd = 43;

		[ActionIdFieldInfo(categoryName = "For controlling debug modules", friendlyName = "Scroll log to start")]
		public const int LogScrollToStart = 44;

		[ActionIdFieldInfo(categoryName = "For controlling debug modules", friendlyName = "Toggle Debug Menu Combo 2")]
		public const int ToggleDebugMenuCombo1 = 60;

		[ActionIdFieldInfo(categoryName = "For controlling debug modules", friendlyName = "Toggle Debug Menu Combo 2")]
		public const int ToggleDebugMenuCombo2 = 61;

		[ActionIdFieldInfo(categoryName = "GraffitiGame", friendlyName = "GraffitiGameCancel")]
		public const int GraffitiGameCancel = 54;

		[ActionIdFieldInfo(categoryName = "Remapping", friendlyName = "RemappingCancel")]
		public const int RemappingCancel = 55;
	}
}
