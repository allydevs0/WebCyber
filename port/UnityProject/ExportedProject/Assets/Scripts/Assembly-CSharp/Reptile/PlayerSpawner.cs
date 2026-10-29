using UnityEngine;

namespace Reptile
{
	public class PlayerSpawner : MonoBehaviour
	{
		public bool isDefaultSpawnPoint;

		public bool isRespawner;

		internal bool isReached;

		private SaveManager saveManager;

		public void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		public void SetReached()
		{
			isReached = true;
			if (GetComponentInChildren<Collider>() != null)
			{
				GetComponentInChildren<Collider>().gameObject.SetActive(value: false);
			}
			Transform transform = base.transform;
			StageProgress currentStageProgress = saveManager.CurrentSaveSlot.GetCurrentStageProgress();
			currentStageProgress.respawnPos = transform.position;
			currentStageProgress.respawnRot = transform.rotation.eulerAngles;
			saveManager.SaveCurrentSaveSlot();
		}

		public void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
		}

		public bool PositionInsideBounds(Vector3 position)
		{
			BoxCollider componentInChildren = GetComponentInChildren<BoxCollider>(includeInactive: true);
			if (componentInChildren != null)
			{
				return PositionIsInside(position, componentInChildren);
			}
			return false;
		}

		private bool PositionIsInside(Vector3 p_Point, BoxCollider p_Box)
		{
			p_Point = p_Box.transform.InverseTransformPoint(p_Point) - p_Box.center;
			float num = p_Box.size.x * 0.5f;
			float num2 = p_Box.size.y * 0.5f;
			float num3 = p_Box.size.z * 0.5f;
			if (p_Point.x < num && p_Point.x > 0f - num && p_Point.y < num2 && p_Point.y > 0f - num2 && p_Point.z < num3)
			{
				return p_Point.z > 0f - num3;
			}
			return false;
		}

		private void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterPlayerSpawner(this);
			saveManager = Core.Instance.SaveManager;
		}
	}
}
