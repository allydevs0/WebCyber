using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public abstract class ASceneSetupInstruction
	{
		protected const int ASSET_STAGE_LOADING = 0;

		protected const int SCENE_STAGE_LOADING = 1;

		protected string sceneName = string.Empty;

		protected bool isStage;

		protected int currentLoadingStage;

		protected float loadingPerStageMultiplier = 1f;

		protected float extendedLoadingTime;

		protected AsyncOperation currentSceneASyncOperation;

		private int maxLoadingStages;

		internal Coroutine SetupCoroutine;

		public string SceneName => sceneName;

		public bool IsStage => isStage;

		public float ExtendedLoadingTime => extendedLoadingTime;

		public ASceneSetupInstruction(string sceneName, int maxLoadingStages, float extendedLoadingTime)
		{
			this.sceneName = sceneName;
			this.maxLoadingStages = maxLoadingStages;
			this.extendedLoadingTime = extendedLoadingTime;
			if (maxLoadingStages > 0)
			{
				CalculateLoadingPerStageMultiplier();
			}
		}

		public abstract IEnumerator ExecuteInstruction(Assets assets, SceneLoader sceneLoader);

		public float GetLoadingProgress(Assets assets)
		{
			if (currentLoadingStage == maxLoadingStages)
			{
				return 1f;
			}
			return CalculateLoadingProgress(assets);
		}

		protected void SetSceneActive(string sceneToSetActive)
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToSetActive));
		}

		protected virtual float CalculateLoadingProgress(Assets assets)
		{
			float num = (float)currentLoadingStage * loadingPerStageMultiplier;
			if (currentLoadingStage == 0)
			{
				num += assets.GetLoadingProgress() * loadingPerStageMultiplier;
			}
			else if (currentLoadingStage == 1)
			{
				num += GetSceneLoadingProgress() * loadingPerStageMultiplier;
			}
			return Mathf.Min(num, 1f);
		}

		protected void SetCurrentLoadingPart(int newLoadingPart)
		{
			currentLoadingStage = newLoadingPart;
		}

		protected void FinishLoading()
		{
			currentLoadingStage = maxLoadingStages;
		}

		private float GetSceneLoadingProgress()
		{
			if (currentSceneASyncOperation == null)
			{
				return 0f;
			}
			return currentSceneASyncOperation.progress;
		}

		private void CalculateLoadingPerStageMultiplier()
		{
			loadingPerStageMultiplier = 1f / (float)maxLoadingStages;
		}
	}
}
