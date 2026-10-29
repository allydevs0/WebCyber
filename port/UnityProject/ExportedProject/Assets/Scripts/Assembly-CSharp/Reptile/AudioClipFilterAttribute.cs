using UnityEngine;

namespace Reptile
{
	public class AudioClipFilterAttribute : PropertyAttribute
	{
		public readonly string propertySearchString;

		public AudioClipFilterAttribute(string sfxCollectionVariableName)
		{
			propertySearchString = sfxCollectionVariableName;
		}
	}
}
