using UnityEngine;

namespace Reptile
{
	public class SfxCollectionPickerAttribute : PropertyAttribute
	{
		public readonly string SearchString;

		public SfxCollectionPickerAttribute(string searchString)
		{
			SearchString = searchString;
		}
	}
}
