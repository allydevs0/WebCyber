using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Reptile
{
	public class StreetLifeCluster : TransformMoveAlongHandler
	{
		public int hideFromWantedStar = 2;

		internal bool offDueToWantedStatus;

		private StreetLifeBehaviour streetLifeBehaviour;

		private BirdBehaviour birdBehaviour;

		private StreetLifeHot[] streetLife;

		private StreetLife[] pedestrians;

		private Bird[] birds;

		private int birdsStartIndex;

		private int distanceCheckIterator;

		public bool distanceOptimize = true;

		private Transform tf;

		private Vector3 clusterPosition = Vector3.zero;

		private float deactivateDistance = 63f;

		private bool shortDistanceLOD;

		private bool clusterActive = true;

		public int StreetLifeClusterCount => streetLife.Length;

		private void Awake()
		{
			tf = base.transform;
			clusterPosition = tf.position;
			InitMoveHandler();
			StageManager.OnStageInitialized += OnStageInitialized;
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		private void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			sceneObjectsRegister.RegisterStreetLifeCluster(this);
		}

		public override void InitMoveHandler()
		{
			MoveAlongPoints[] componentsInChildren = base.transform.GetComponentsInChildren<MoveAlongPoints>();
			moversHot = new List<MoveAlongPoints_Hot>(componentsInChildren.Length);
			moversCold = componentsInChildren.ToList();
			int count = moversCold.Count;
			for (int i = 0; i < count; i++)
			{
				MoveAlongPoints_Hot moverHot = default;
				MoveAlongPoints moveAlongPoints = moversCold[i];
				moverHot = SetPath(moverHot, moveAlongPoints.path, null);
				moverHot = componentsInChildren[i].InitHotData(this, moverHot);
				moverHot = StartFromPoint(moverHot, moveAlongPoints.delay, moveAlongPoints.delayMultiplier, i, moveAlongPoints.startOnRandomPoint ? Random.Range(0, moverHot.pathPositions.Length - 1) : 0);
				moversHot.Add(moverHot);
			}
		}

		private void OnStageInitialized()
		{
			List<StreetLife> list = tf.GetComponentsInChildren<StreetLife>(includeInactive: true).ToList();
			List<Bird> list2 = new List<Bird>();
			List<StreetLife> list3 = new List<StreetLife>();
			for (int i = 0; i < list.Count; i++)
			{
				Bird component = list[i].GetComponent<Bird>();
				if (component == null)
				{
					list3.Add(list[i]);
				}
				else
				{
					list2.Add(component);
				}
			}
			pedestrians = list3.ToArray();
			birds = list2.ToArray();
			int num = pedestrians.Length;
			int num2 = birds.Length;
			streetLife = new StreetLifeHot[pedestrians.Length + birds.Length];
			birdsStartIndex = num;
			streetLifeBehaviour = new StreetLifeBehaviour();
			birdBehaviour = new BirdBehaviour(birds.Length);
			for (int j = 0; j < num; j++)
			{
				StreetLifeHot streetLifeHot = pedestrians[j].Init(this, streetLifeBehaviour, j);
				streetLife[j] = streetLifeHot;
			}
			for (int k = 0; k < num2; k++)
			{
				StreetLifeHot streetLifeHot2 = birds[k].Init(this, birdBehaviour, birdsStartIndex + k);
				streetLife[birdsStartIndex + k] = streetLifeHot2;
			}
			shortDistanceLOD = QualitySettings.lodBias < 2f;
			deactivateDistance *= deactivateDistance;
			if (shortDistanceLOD)
			{
				deactivateDistance *= 0.5f;
			}
			Core.OnCoreUpdatePaused += OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused += OnCoreUpdateUnPaused;
		}

		internal static void SetClusterActiveIfAllowed(StreetLifeCluster cluster, int wantedStars, bool combatEncounterActive)
		{
			if (cluster.hideFromWantedStar == -1 || (!cluster.offDueToWantedStatus && wantedStars < cluster.hideFromWantedStar && !combatEncounterActive))
			{
				cluster.SetClusterActive(set: true);
			}
		}

		internal static void ForceStreetlifeResetWhenInRange(StreetLifeCluster cluster, Vector3 position)
		{
			int num = cluster.streetLife.Length;
			for (int i = 0; i < num; i++)
			{
				if ((cluster.streetLife[i].transform.position - position).sqrMagnitude < cluster.deactivateDistance)
				{
					cluster.streetLife[i].activationGameObject.SetActive(value: true);
					cluster.streetLifeBehaviour.ResetBack(cluster, ref cluster.streetLife[i], restartPath: false);
				}
			}
		}

		public void UpdateStreetLifeCluster(Vector3 currentCameraPosition, int wantedStars)
		{
			bool flag = WorldHandler.instance.currentEncounter is CombatEncounter;
			if (offDueToWantedStatus && (hideFromWantedStar == -1 || (wantedStars < hideFromWantedStar && !flag)))
			{
				offDueToWantedStatus = false;
			}
			SetClusterActiveIfAllowed(this, wantedStars, flag);
			if (hideFromWantedStar != -1 && ((wantedStars >= hideFromWantedStar) | flag))
			{
				offDueToWantedStatus = true;
				SetClusterActive(set: false);
			}
			if (clusterActive)
			{
				UpdateMovers();
				UpdateStreetLife();
			}
		}

		public void SetMoverOff(MoveAlongPoints moveAlongPoints, bool setOff)
		{
			int index = moversCold.IndexOf(moveAlongPoints);
			MoveAlongPoints_Hot value = moversHot[index];
			value.turnedOff = setOff;
			moversHot[index] = value;
		}

		public void RestartMoverPath(MoveAlongPoints moveAlongPoints)
		{
			int num = moversCold.IndexOf(moveAlongPoints);
			MoveAlongPoints_Hot moverHot = moversHot[num];
			MoveAlongPoints moveAlongPoints2 = moversCold[num];
			moversHot[num] = StartFromPoint(moverHot, moveAlongPoints2.delay, moveAlongPoints2.delayMultiplier, num);
		}

		public Vector3 GetMoverLastPos(MoveAlongPoints moveAlongPoints)
		{
			int index = moversCold.IndexOf(moveAlongPoints);
			return moversHot[index].lastPos;
		}

		public void SetClusterActive(bool set)
		{
			if (set == clusterActive)
			{
				return;
			}
			if (!set)
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				base.gameObject.SetActive(value: true);
				Vector3 currentCameraPosition = WorldHandler.instance.CurrentCameraPosition;
				int num = streetLife.Length;
				for (int i = 0; i < num; i++)
				{
					if ((streetLife[i].transform.position - currentCameraPosition).sqrMagnitude < deactivateDistance)
					{
						streetLife[i].activationGameObject.SetActive(value: true);
						streetLifeBehaviour.ResetBack(this, ref streetLife[i], restartPath: false);
					}
				}
			}
			clusterActive = set;
		}

		internal void ResetStreetlifeIfActive()
		{
			if (!clusterActive || streetLife == null)
			{
				return;
			}
			Vector3 currentCameraPosition = WorldHandler.instance.CurrentCameraPosition;
			int num = streetLife.Length;
			for (int i = 0; i < num; i++)
			{
				if ((streetLife[i].transform.position - currentCameraPosition).sqrMagnitude < deactivateDistance)
				{
					streetLife[i].activationGameObject.SetActive(value: true);
					streetLifeBehaviour.ResetBack(this, ref streetLife[i], restartPath: false);
				}
			}
		}

		private void OnCoreUpdatePaused()
		{
			int num = streetLife.Length;
			for (int i = 0; i < num; i++)
			{
				streetLife[i].anim.speed = 0f;
			}
		}

		private void OnCoreUpdateUnPaused()
		{
			int num = streetLife.Length;
			for (int i = 0; i < num; i++)
			{
				streetLife[i].anim.speed = 1f;
			}
		}

		private void UpdateStreetLife()
		{
			if (distanceOptimize && distanceCheckIterator < streetLife.Length)
			{
				float sqrMagnitude = (streetLife[distanceCheckIterator].transform.position - WorldHandler.instance.CurrentCameraPosition).sqrMagnitude;
				if (streetLife[distanceCheckIterator].activationGameObject.activeSelf)
				{
					if (sqrMagnitude >= deactivateDistance)
					{
						streetLife[distanceCheckIterator].activationGameObject.SetActive(value: false);
					}
				}
				else if (sqrMagnitude < deactivateDistance)
				{
					streetLife[distanceCheckIterator].activationGameObject.SetActive(value: true);
					streetLifeBehaviour.ResetBack(this, ref streetLife[distanceCheckIterator], restartPath: false);
				}
				distanceCheckIterator++;
				if (distanceCheckIterator >= streetLife.Length)
				{
					distanceCheckIterator = 0;
				}
			}
			if (base.gameObject.activeInHierarchy)
			{
				streetLifeBehaviour.UpdateStreetLife(this, streetLife, 0, birdsStartIndex);
				birdBehaviour.UpdateStreetLife(this, streetLife, birdsStartIndex, streetLife.Length - birdsStartIndex);
			}
		}

		private void UpdateMovers()
		{
			Vector3 position = WorldHandler.instance.GetCurrentPlayer().tf.position;
			float dt = Core.dt;
			bool isBusy = SequenceHandler.instance.isBusy;
			UpdateMovingObjects(dt, position, isBusy);
		}

		public StreetLifeHot GetStreetLifeHot(int index)
		{
			return streetLife[index];
		}

		public void SetStreetLifeHot(int index, StreetLifeHot streetLifeHot)
		{
			streetLife[index] = streetLifeHot;
		}

		public StreetLifeBehaviour GetStreetLifeBehaviour()
		{
			return streetLifeBehaviour;
		}

		public void StreetLifeMove(int indexIntoCluster, StreetLife life, Transform otherTransform)
		{
			if (indexIntoCluster >= birdsStartIndex)
			{
				birdBehaviour.Move(this, life, ref streetLife[indexIntoCluster], otherTransform);
			}
			else
			{
				streetLifeBehaviour.Move(this, life, ref streetLife[indexIntoCluster], otherTransform);
			}
		}

		public void OnEnable()
		{
			if (streetLife != null)
			{
				int num = streetLife.Length;
				for (int i = 0; i < num; i++)
				{
					streetLifeBehaviour.ResetBack(this, ref streetLife[i], restartPath: false);
				}
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			Core.OnCoreUpdatePaused -= OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused -= OnCoreUpdateUnPaused;
			StageManager.OnStageInitialized -= OnStageInitialized;
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			streetLife = null;
			pedestrians = null;
			birds = null;
			tf = null;
			streetLifeBehaviour = null;
			birdBehaviour = null;
		}
	}
}
