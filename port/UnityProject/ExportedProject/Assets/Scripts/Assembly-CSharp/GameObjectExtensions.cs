using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
	public static RectTransform RectTransform(this GameObject gameObject)
	{
		return gameObject.GetComponent<RectTransform>();
	}

	public static GameObject[] GetChildrenByLayer(this GameObject gameObject, int layer)
	{
		List<GameObject> list = new List<GameObject>();
		if (gameObject.layer == layer)
		{
			list.Add(gameObject);
		}
		foreach (Transform item in gameObject.transform)
		{
			list.AddRange(item.gameObject.GetChildrenByLayer(layer));
		}
		return list.ToArray();
	}
}
