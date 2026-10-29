using Rewired;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "PlatformData", menuName = "Reptile/Platform/Platform data", order = 0)]
	public class PlatformData : ScriptableObject
	{
		[Tooltip("Assign the platform dependant button sprite data here")]
		public ButtonSpriteData buttonSpriteData;

		public PlatformLanguages languageData;

		public string defaultControllerGuid;

		public ControllerType defaultControllerType = ControllerType.Joystick;
	}
}
