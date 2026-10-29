using UnityEngine;

namespace Reptile
{
	public class StageChunk : MonoBehaviour, IStaticMeshCombineTarget
	{
		private delegate bool FilterCheckTypeFunction<T>(StageChunk childChunk, T junkToCheck);

		private float timer;

		private BoxCollider[] colliders;

		internal Junk[] junkOnlyInThisChunk;

		internal StreetLifeCluster[] streetLifeClusters;

		internal MoveObject[] moveObjects;

		internal bool camIsInChunk;

		public Transform TriggerHolder;

		private JunkBehaviour junkBehaviour;

		private bool isActive;

		private bool initialized;

		public void Awake()
		{
			isActive = true;
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		private void OnStageInitialized()
		{
			if (!initialized)
			{
				InitStageChunk();
			}
		}

		internal void InitStageChunk()
		{
			if (initialized)
			{
				return;
			}
			Core.OnUpdate += UpdateChunk;
			WorldHandler.instance.SceneObjectsRegister.RegisterStageChunk(this);
			if (TriggerHolder == null)
			{
				colliders = GetComponents<BoxCollider>();
			}
			else
			{
				colliders = TriggerHolder.GetComponentsInChildren<BoxCollider>();
			}
			StageChunk[] componentsInChildren = GetComponentsInChildren<StageChunk>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i] != this)
				{
					componentsInChildren[i].InitStageChunk();
				}
			}
			Junk[] componentsInChildren2 = GetComponentsInChildren<Junk>(includeInactive: true);
			StreetLifeCluster[] componentsInChildren3 = GetComponentsInChildren<StreetLifeCluster>();
			MoveObject[] componentsInChildren4 = GetComponentsInChildren<MoveObject>();
			if (componentsInChildren.Length > 1)
			{
				FilterJunk(componentsInChildren, componentsInChildren2, ref junkOnlyInThisChunk);
				FilterStreetLifeCluster(componentsInChildren, componentsInChildren3, ref streetLifeClusters);
				FilterMoveObjects(componentsInChildren, componentsInChildren4, ref moveObjects);
			}
			else
			{
				junkOnlyInThisChunk = componentsInChildren2;
				streetLifeClusters = componentsInChildren3;
				moveObjects = componentsInChildren4;
			}
			junkBehaviour = new JunkBehaviour();
			junkBehaviour.Init(junkOnlyInThisChunk);
			for (int j = 0; j < moveObjects.Length; j++)
			{
				moveObjects[j].Init();
			}
			initialized = true;
		}

		private void FilterMoveObjects(StageChunk[] childChunks, MoveObject[] allMoveObjectsInChunk, ref MoveObject[] resultMoveObjectsArray)
		{
			int num = 0;
			for (int i = 0; i < childChunks.Length; i++)
			{
				if (childChunks[i] != this)
				{
					num += childChunks[i].moveObjects.Length;
				}
			}
			FilterTypeArray(childChunks, allMoveObjectsInChunk, ref resultMoveObjectsArray, num, FilterMoveObjectsChecker);
		}

		private void FilterStreetLifeCluster(StageChunk[] childChunks, StreetLifeCluster[] allClustersInChunk, ref StreetLifeCluster[] resultClustersArray)
		{
			int num = 0;
			for (int i = 0; i < childChunks.Length; i++)
			{
				if (childChunks[i] != this)
				{
					num += childChunks[i].streetLifeClusters.Length;
				}
			}
			FilterTypeArray(childChunks, allClustersInChunk, ref resultClustersArray, num, FilterStreetlifeClusterChecker);
		}

		private void FilterJunk(StageChunk[] childChunks, Junk[] allJunkInChunk, ref Junk[] resultJunkArray)
		{
			int num = 0;
			for (int i = 0; i < childChunks.Length; i++)
			{
				if (childChunks[i] != this)
				{
					num += childChunks[i].junkOnlyInThisChunk.Length;
				}
			}
			FilterTypeArray(childChunks, allJunkInChunk, ref resultJunkArray, num, FilterJunkChecker);
		}

		private bool FilterJunkChecker(StageChunk childChunk, Junk junkToCheck)
		{
			bool result = false;
			Junk[] array = childChunk.junkOnlyInThisChunk;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == junkToCheck)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		private bool FilterStreetlifeClusterChecker(StageChunk childChunk, StreetLifeCluster streetLifeClusterToCheck)
		{
			bool result = false;
			StreetLifeCluster[] array = childChunk.streetLifeClusters;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == streetLifeClusterToCheck)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		private bool FilterMoveObjectsChecker(StageChunk childChunk, MoveObject moveObjectToCheck)
		{
			bool result = false;
			MoveObject[] array = childChunk.moveObjects;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == moveObjectToCheck)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		private void FilterTypeArray<T>(StageChunk[] childChunks, T[] allJunkInChunk, ref T[] resultJunkArray, int totalJunkInChildren, FilterCheckTypeFunction<T> checkFunction)
		{
			int num = 0;
			resultJunkArray = new T[allJunkInChunk.Length - totalJunkInChildren];
			for (int i = 0; i < allJunkInChunk.Length; i++)
			{
				bool flag = false;
				T val = allJunkInChunk[i];
				for (int j = 0; j < childChunks.Length; j++)
				{
					if (childChunks[j] != this && checkFunction(childChunks[j], val))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					resultJunkArray[num++] = val;
				}
			}
		}

		internal static bool PointInOBB(Vector3 point, BoxCollider box)
		{
			Vector3 size = box.size;
			point = box.transform.InverseTransformPoint(point) - box.center;
			float num = size.x * 0.5f;
			float num2 = size.y * 0.5f;
			float num3 = size.z * 0.5f;
			if (point.x < num && point.x > 0f - num && point.y < num2 && point.y > 0f - num2 && point.z < num3)
			{
				return point.z > 0f - num3;
			}
			return false;
		}

		internal static void ActivateIfPointIsInChunk(StageChunk chunk, Vector3 position)
		{
			if (!chunk.isActive)
			{
				int num = chunk.colliders.Length;
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					if (flag)
					{
						break;
					}
					flag = PointInOBB(position, chunk.colliders[i]);
				}
				if (flag != chunk.camIsInChunk)
				{
					chunk.camIsInChunk = flag;
					if (flag)
					{
						chunk.timer = 1f;
						chunk.ActivateChunk(set: true);
					}
				}
			}
			if (chunk.isActive)
			{
				int wantedStars = WantedManager.instance.WantedStars;
				bool combatEncounterActive = WorldHandler.instance.currentEncounter is CombatEncounter;
				int num2 = chunk.streetLifeClusters.Length;
				for (int j = 0; j < num2; j++)
				{
					StreetLifeCluster.SetClusterActiveIfAllowed(chunk.streetLifeClusters[j], wantedStars, combatEncounterActive);
					StreetLifeCluster.ForceStreetlifeResetWhenInRange(chunk.streetLifeClusters[j], position);
				}
			}
		}

		public void UpdateChunk()
		{
			camIsInChunk = false;
			Vector3 currentCameraPosition = WorldHandler.instance.CurrentCameraPosition;
			int num = colliders.Length;
			for (int i = 0; i < num; i++)
			{
				if (PointInOBB(currentCameraPosition, colliders[i]))
				{
					camIsInChunk = true;
					timer = 1f;
					break;
				}
			}
			if (!isActive)
			{
				if (camIsInChunk)
				{
					ActivateChunk(set: true);
				}
				return;
			}
			Vector3 currentCameraPosition2 = WorldHandler.instance.CurrentCameraPosition;
			UpdateStreetLife(currentCameraPosition2);
			UpdateMoveObjects(currentCameraPosition2);
			JunkBehaviour.JunkUpdate(junkBehaviour, Core.dt);
			if (!camIsInChunk)
			{
				timer -= Core.dt;
				if (timer <= 0f)
				{
					ActivateChunk(set: false);
				}
			}
		}

		public void ActivateChunk(bool set)
		{
			isActive = set;
			base.gameObject.SetActive(set);
			if (set)
			{
				JunkBehaviour.RestoreAll(junkBehaviour);
				return;
			}
			int num = streetLifeClusters.Length;
			for (int i = 0; i < num; i++)
			{
				streetLifeClusters[i].SetClusterActive(set: false);
			}
		}

		private void UpdateStreetLife(Vector3 currentCameraPosition)
		{
			int wantedStars = WantedManager.instance.WantedStars;
			int num = streetLifeClusters.Length;
			for (int i = 0; i < num; i++)
			{
				streetLifeClusters[i].UpdateStreetLifeCluster(currentCameraPosition, wantedStars);
			}
		}

		private void UpdateMoveObjects(Vector3 currentCameraPosition)
		{
			for (int i = 0; i < moveObjects.Length; i++)
			{
				moveObjects[i].UpdateMoveObject(currentCameraPosition);
			}
		}

		public void OnDestroy()
		{
			StageManager.OnStageInitialized -= OnStageInitialized;
			Core.OnUpdate -= UpdateChunk;
			junkBehaviour = null;
			TriggerHolder = null;
			streetLifeClusters = null;
			moveObjects = null;
			colliders = null;
		}
	}
}
