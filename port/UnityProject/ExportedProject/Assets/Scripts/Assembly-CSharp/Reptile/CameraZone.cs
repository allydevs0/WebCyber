using UnityEngine;

namespace Reptile
{
	public class CameraZone : MonoBehaviour
	{
		public Transform fixedCameraPoint;

		public string showAreaName = string.Empty;

		public string localizationKey = string.Empty;

		public bool attachedToStageExit;

		public CameraZoneMode cameraMode = CameraZoneMode.FIXED;

		public BoxSide planeLimitedSide;

		public bool cylinderPlaneLimited;

		public bool onlyOnGrinds;

		internal BoxCollider box;

		private IGameTextLocalizer localizer;

		private Stage cameraZoneStage = Stage.NONE;

		[Tooltip("For FIXED_ANGLE mode, the camera viewing angle (0 = blue arrow of zone box, positive = turned clockwise)")]
		public float fixedAngle;

		[Tooltip("For FIXED_ANGLE mode, the camera distance from the player")]
		public float fixedAngleDistance;

		public void Awake()
		{
			box = GetComponentInChildren<BoxCollider>();
			localizer = Core.Instance.Localizer;
			if (!localizationKey.Equals(string.Empty))
			{
				showAreaName = localizer.GetUserInterfaceText(localizationKey);
			}
			localizer.OnLanguageChanged += OnLanguageChanged;
		}

		public void SetCameraZoneStageLocalization(Stage stage)
		{
			cameraZoneStage = stage;
			showAreaName = localizer.GetStageName(stage);
		}

		public Vector3 GetPlaneLimitNormal()
		{
			Vector3 result = box.transform.forward;
			if (planeLimitedSide == BoxSide.BACK)
			{
				result = box.transform.back();
			}
			if (planeLimitedSide == BoxSide.DOWN)
			{
				result = box.transform.down();
			}
			if (planeLimitedSide == BoxSide.UP)
			{
				result = box.transform.up;
			}
			if (planeLimitedSide == BoxSide.RIGHT)
			{
				result = box.transform.right;
			}
			if (planeLimitedSide == BoxSide.LEFT)
			{
				result = box.transform.left();
			}
			return result;
		}

		public float GetPlaneLimitExtent()
		{
			float num = box.size.z * box.transform.lossyScale.z;
			if (planeLimitedSide == BoxSide.DOWN || planeLimitedSide == BoxSide.UP)
			{
				num = box.size.y * box.transform.lossyScale.y;
			}
			if (planeLimitedSide == BoxSide.RIGHT || planeLimitedSide == BoxSide.LEFT)
			{
				num = box.size.x * box.transform.lossyScale.x;
			}
			return num * 0.5f;
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			if (cameraZoneStage != Stage.NONE)
			{
				showAreaName = localizer.GetStageName(cameraZoneStage);
			}
			else if (!localizationKey.Equals(string.Empty))
			{
				showAreaName = localizer.GetUserInterfaceText(localizationKey);
			}
		}

		private void OnDisable()
		{
			WorldHandler instance = WorldHandler.instance;
			if (instance == null)
			{
				return;
			}
			Player currentPlayer = instance.GetCurrentPlayer();
			if (!(currentPlayer == null))
			{
				GameplayCamera cam = currentPlayer.cam;
				if (!(cam == null))
				{
					cam.cameraZone = null;
				}
			}
		}

		private void OnDestroy()
		{
			localizer.OnLanguageChanged -= OnLanguageChanged;
		}
	}
}
