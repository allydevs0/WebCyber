using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public class LoadSceneInstruction : ASceneSetupInstruction
	{
		private const int MAX_LOADING_PARTS = 2;

		private bool setActiveAfterLoad;

		public LoadSceneInstruction(string sceneName, bool setActiveAfterLoad)
			: base(sceneName, 2, 0f)
		{
			this.setActiveAfterLoad = setActiveAfterLoad;
		}

		public override IEnumerator ExecuteInstruction(Assets assets, SceneLoader sceneLoader)
		{
			SetCurrentLoadingPart(0);
			yield return assets.LoadAssetBundlesForSceneASync(sceneName);
			yield return assets.LoadAssetLoadersForSceneASync(sceneName);
			SetCurrentLoadingPart(1);
			yield return sceneLoader.LoadScene(sceneName, LoadSceneMode.Additive);
			FinishLoading();
			yield return new WaitForSeconds(0.25f);
			if (setActiveAfterLoad)
			{
				SetSceneActive(sceneName);
			}
		}
	}
}
