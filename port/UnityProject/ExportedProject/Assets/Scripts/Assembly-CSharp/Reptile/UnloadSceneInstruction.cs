using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class UnloadSceneInstruction : ASceneSetupInstruction
	{
		private const int MAX_LOADING_PARTS = 2;

		private string sceneNameToSetActive = string.Empty;

		public UnloadSceneInstruction(string sceneName, string sceneNameToSetActive)
			: base(sceneName, 2, 0f)
		{
			this.sceneNameToSetActive = sceneNameToSetActive;
		}

		public override IEnumerator ExecuteInstruction(Assets assets, SceneLoader sceneLoader)
		{
			assets.UnloadAssetLoadersForScene(sceneName);
			assets.UnloadAssetBundlesForScene(sceneName);
			currentSceneASyncOperation = sceneLoader.UnloadScene(sceneName);
			yield return currentSceneASyncOperation;
			FinishLoading();
			yield return new WaitForSeconds(0.25f);
			CleanupReferences();
			Resources.UnloadUnusedAssets();
			if (!sceneNameToSetActive.Equals(string.Empty))
			{
				SetSceneActive(sceneNameToSetActive);
			}
		}

		protected override float CalculateLoadingProgress(Assets assets)
		{
			return 0f;
		}

		private void CleanupReferences()
		{
			Core.Instance.CleanupUpdateEvents();
		}
	}
}
