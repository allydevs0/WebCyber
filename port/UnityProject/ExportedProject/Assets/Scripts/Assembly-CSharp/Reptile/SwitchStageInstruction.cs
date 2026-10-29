using System.Collections;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public class SwitchStageInstruction : ASceneSetupInstruction
	{
		private const int MAX_LOADING_PARTS = 1;

		private Stage newStage = Stage.NONE;

		private Stage previousStage = Stage.NONE;

		private UnloadStageASyncInstruction currentUnloadStageASyncInstuction;

		private LoadStageASyncInstruction currentLoadStageASyncInstruction;

		private string sceneToSetActiveOnUnload = string.Empty;

		public Stage NewStage => newStage;

		public Stage PreviousStage => previousStage;

		public SwitchStageInstruction(Stage newStage, Stage previousStage, float extendedLoadingTime, string sceneToSetActiveOnUnload)
			: base(newStage.ToString(), 1, extendedLoadingTime)
		{
			this.newStage = newStage;
			this.previousStage = previousStage;
			this.sceneToSetActiveOnUnload = sceneToSetActiveOnUnload;
			isStage = true;
		}

		public override IEnumerator ExecuteInstruction(Assets assets, SceneLoader sceneLoader)
		{
			currentUnloadStageASyncInstuction = new UnloadStageASyncInstruction(Utility.SceneNameToStage(SceneManager.GetActiveScene().name), sceneToSetActiveOnUnload);
			currentLoadStageASyncInstruction = new LoadStageASyncInstruction(newStage, setActiveAfterLoad: true, extendedLoadingTime);
			yield return currentUnloadStageASyncInstuction.ExecuteInstruction(assets, sceneLoader);
			yield return currentLoadStageASyncInstruction.ExecuteInstruction(assets, sceneLoader);
			FinishLoading();
		}

		protected override float CalculateLoadingProgress(Assets assets)
		{
			return currentLoadStageASyncInstruction.GetLoadingProgress(assets) * loadingPerStageMultiplier;
		}
	}
}
