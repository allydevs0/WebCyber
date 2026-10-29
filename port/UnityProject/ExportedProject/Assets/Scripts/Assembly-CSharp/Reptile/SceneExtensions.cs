using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public static class SceneExtensions
	{
		public static List<T> FindObjectsOfType<T>(this Scene scene, bool includeInactive)
		{
			List<T> list = new List<T>();
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive));
			}
			return list;
		}
	}
}
