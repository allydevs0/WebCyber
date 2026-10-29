using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class PlayerAIPath : MonoBehaviour
	{
		public PlayerAIPath nextPath;

		internal List<PlayerAIWaypoint> waypoints = new List<PlayerAIWaypoint>();

		internal PlayerAIWaypoint firstWaypoint;

		internal PlayerAIWaypoint lastWaypoint;

		public int pathID = -1;

		public int nextPathID = -1;

		public void Awake()
		{
			foreach (Transform item in base.transform)
			{
				waypoints.Add(item.GetComponent<PlayerAIWaypoint>());
			}
			firstWaypoint = waypoints[0];
			lastWaypoint = waypoints[waypoints.Count - 1];
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		public PlayerAIWaypoint GetNextWaypoint(PlayerAIWaypoint curWaypoint)
		{
			return waypoints[waypoints.IndexOf(curWaypoint) + 1];
		}

		public bool ChunkIsActive()
		{
			return base.gameObject.activeInHierarchy;
		}

		protected void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterPath(this);
		}

		public void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
		}
	}
}
