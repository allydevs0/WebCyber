using UnityEngine;

namespace Reptile
{
	public class CameraModeGrind : CameraMode
	{
		private GrindAbility grindAbility;

		public CameraModeGrind(GrindAbility grindAbility)
		{
			this.grindAbility = grindAbility;
		}

		public override void Activate(Player player, CameraMode modePrev)
		{
			base.Activate(player, modePrev);
			inputEnabled = true;
		}

		protected override void UpdatePos(int fixedFrameCounter)
		{
			HandleLookAt(fixedFrameCounter);
			position = HandleCamInput(position);
			Vector3 dir = player.dir;
			dir.y = 0f;
			float num = Mathf.Atan2(player.dir.y, Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z)) * 57.29578f;
			float num2 = Mathf.Clamp(cam.grindHeightOffsetDefault - num / cam.grindHeightOffsetAngleQuotient, cam.grindHeightOffsetMin, cam.grindHeightOffsetMax);
			if (player.tf.up.y < 0f)
			{
				num2 *= player.tf.up.y + 1f;
			}
			position = HandleDragging(position, num2, cam.grindPushBehind);
			positionFinal = HandleObstructions(position);
			float target = 0f;
			if (Mathf.Abs(grindAbility.grindTilt.x) > 0.5f)
			{
				target = (0f - Mathf.Sign(grindAbility.grindTilt.x)) * cam.grindTiltMax;
			}
			tilt = LerpTo(tilt, target, cam.grindTiltSpeed);
		}
	}
}
