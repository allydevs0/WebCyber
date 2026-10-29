using System.Collections;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public class LoadSceneASyncInstruction : ASceneSetupInstruction
	{
		private const int MAX_LOADING_PARTS = 2;

		protected bool setActiveAfterLoad;

		public LoadSceneASyncInstruction(string sceneName, bool setActiveAfterLoad, float extendedLoadingTime)
			: base(sceneName, 2, extendedLoadingTime)
		{
			this.setActiveAfterLoad = setActiveAfterLoad;
		}

		public override IEnumerator ExecuteInstruction(Assets assets, SceneLoader sceneLoader)
		{
			SetCurrentLoadingPart(0);
			yield return assets.LoadAssetBundlesForSceneASync(sceneName);
			yield return assets.LoadAssetLoadersForSceneASync(sceneName);
			currentSceneASyncOperation = sceneLoader.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			SetCurrentLoadingPart(1);
			while (!currentSceneASyncOperation.isDone || UIPopup.waitForPopupToFinish || BaseModule.waitForSaveAndLogout)
			{
				yield return null;
			}
			FinishLoading();
			if (setActiveAfterLoad)
			{
				SetSceneActive(sceneName);
			}
		}
	}
}
