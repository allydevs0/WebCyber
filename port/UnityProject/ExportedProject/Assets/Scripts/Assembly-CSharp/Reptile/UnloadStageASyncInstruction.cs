namespace Reptile
{
	public class UnloadStageASyncInstruction : UnloadSceneInstruction
	{
		public UnloadStageASyncInstruction(Stage stage, string sceneNameToSetActive)
			: base(stage.ToString(), sceneNameToSetActive)
		{
			isStage = true;
		}
	}
}
