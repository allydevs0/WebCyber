namespace Reptile
{
	public class LoadStageASyncInstruction : LoadSceneASyncInstruction
	{
		public LoadStageASyncInstruction(Stage stage, bool setActiveAfterLoad, float extendedLoadingTime)
			: base(stage.ToString(), setActiveAfterLoad, extendedLoadingTime)
		{
			isStage = true;
		}
	}
}
