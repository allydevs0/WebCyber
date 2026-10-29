namespace Reptile
{
	public class CameraModeDrag : CameraMode
	{
		public override void Activate(Player player, CameraMode modePrev)
		{
			base.Activate(player, modePrev);
			inputEnabled = true;
			if (modePrev is CameraModeVert)
			{
				Reset();
			}
		}

		protected override void UpdatePos(int fixedFrameCounter)
		{
			HandleLookAt(fixedFrameCounter);
			position = HandleCamInput(position);
			position = HandleDragging(position);
			positionFinal = HandleObstructions(position);
		}
	}
}
