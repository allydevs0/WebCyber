using UnityEngine;

namespace Reptile
{
	public class PreludeMusicStopper : MonoBehaviour
	{
		[SerializeField]
		internal ProgressObject startProgressObject;

		[SerializeField]
		internal ProgressObject countdownProgressObject;

		private void Awake()
		{
			StageManager.OnStagePostInitialization += StageManager_OnStageInitialized;
		}

		private void OnDestroy()
		{
			StageManager.OnStagePostInitialization -= StageManager_OnStageInitialized;
		}

		private void StageManager_OnStageInitialized()
		{
			StageProgress stageProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetStageProgress(Stage.Prelude);
			ObjectProgress objectProgress = (ObjectProgress)stageProgress.GetProgressableData(startProgressObject.Uid);
			ObjectProgress objectProgress2 = (ObjectProgress)stageProgress.GetProgressableData(countdownProgressObject.Uid);
			if ((objectProgress != null && objectProgress.isActive) || (objectProgress2 != null && objectProgress2.isActive))
			{
				Core.Instance.AudioManager.MusicPlayer.Pause();
			}
		}
	}
}
