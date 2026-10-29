using System;
using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(Teleport))]
	public class StageTransition : AProgressable, IInitializableSceneObject
	{
		public bool isActive = true;

		public bool allowEntry = true;

		[Header("This also lets you in if specified:")]
		public Story.Chapter entryFromChapter = Story.Chapter.NONE;

		private Teleport teleport;

		[SerializeField]
		[HideInInspector]
		private string sceneToLoad = string.Empty;

		[SerializeField]
		[HideInInspector]
		private PlayerSpawner playerSpawner;

		public Stage[] alsoUseIfComingFromTheseStages;

		private IGameTextLocalizer localizer;

		public PlayerSpawner PlayerSpawner
		{
			get
			{
				return playerSpawner;
			}
			set
			{
				playerSpawner = value;
			}
		}

		public string SceneToLoad => sceneToLoad;

		public override void Awake()
		{
			base.Awake();
			localizer = Core.Instance.Localizer;
		}

		public void InitSceneObject()
		{
			base.gameObject.SetActive(isActive);
			teleport = GetComponent<Teleport>();
			if (teleport.teleportTo == null && playerSpawner != null)
			{
				teleport.teleportTo = playerSpawner.transform;
			}
			CameraZone componentInChildren = base.transform.GetComponentInChildren<CameraZone>();
			if ((bool)componentInChildren)
			{
				Stage cameraZoneStageLocalization = Utility.SceneNameToStage(sceneToLoad);
				componentInChildren.SetCameraZoneStageLocalization(cameraZoneStageLocalization);
				componentInChildren.attachedToStageExit = true;
			}
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
			progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.STAGE_TRANSITION_PROGRESS);
			StageTransitionProgress obj = (StageTransitionProgress)progressableData;
			obj.isActive = isActive;
			obj.allowEntry = allowEntry;
		}

		public override void ReadFromData()
		{
			StageTransitionProgress stageTransitionProgress = (StageTransitionProgress)progressableData;
			isActive = stageTransitionProgress.isActive;
			allowEntry = stageTransitionProgress.allowEntry;
			base.gameObject.SetActive(stageTransitionProgress.isActive);
		}

		public override void WriteToData()
		{
			StageTransitionProgress obj = (StageTransitionProgress)progressableData;
			obj.isActive = isActive;
			obj.allowEntry = allowEntry;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void AllowEntry()
		{
			allowEntry = true;
			isActive = true;
			base.gameObject.SetActive(value: true);
			WriteToData();
		}

		public void AllowEntryWithoutSave()
		{
			allowEntry = true;
			isActive = true;
			base.gameObject.SetActive(value: true);
		}

		public void DisallowEntry()
		{
			allowEntry = false;
			WriteToData();
		}

		public void MakeActive()
		{
			isActive = true;
			base.gameObject.SetActive(value: true);
			WriteToData();
		}

		public void MakeUnactive()
		{
			isActive = false;
			base.gameObject.SetActive(value: false);
			WriteToData();
		}

		protected override void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			base.OnRegisterToSceneObjectsRegister(sceneObjectsRegister);
			sceneObjectsRegister.RegisterStageTransition(this);
		}

		public Stage GetStageToLoad()
		{
			Stage result = Stage.NONE;
			Enum.TryParse<Stage>(sceneToLoad, out result);
			return result;
		}

		public bool CanAlsoComeInFromStage(Stage stage)
		{
			for (int i = 0; i < alsoUseIfComingFromTheseStages.Length; i++)
			{
				if (alsoUseIfComingFromTheseStages[i] == stage)
				{
					return true;
				}
			}
			return false;
		}

		public void PlayerHitTrigger(Player player, StageManager stageManager)
		{
			if (WorldHandler.instance.currentEncounter == null && (allowEntry || ChapterReached(entryFromChapter)))
			{
				stageManager.ExitCurrentStage(GetStageToLoad());
			}
			else
			{
				TeleportBackToSpawner(player);
			}
		}

		private bool ChapterReached(Story.Chapter c)
		{
			if (c == Story.Chapter.NONE)
			{
				return false;
			}
			return Story.GetCurrentObjectiveInfo().chapter >= entryFromChapter;
		}

		public void TransitionToStage()
		{
			Core.Instance.BaseModule.StageManager?.ExitCurrentStage(GetStageToLoad());
		}

		public void TeleportBackToSpawner(Player player)
		{
			teleport.DoTeleport(player);
		}
	}
}
