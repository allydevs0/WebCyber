using UnityEngine;

namespace Reptile
{
	public static class ComponentExtensions
	{
		public static RectTransform RectTransform(this Component component)
		{
			return component.GetComponent<RectTransform>();
		}
	}
}
