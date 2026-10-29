using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "PlatformLanguages", menuName = "Reptile/Platform/Platform languages", order = 1)]
	public class PlatformLanguages : ScriptableObject
	{
		[Tooltip("Set the languages that are available on this platform")]
		public SystemLanguage[] availableLanguages;

		public string[] availableLanguagesNames;

		public string[] availableLanguagesApplyText;
	}
}
