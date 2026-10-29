using UnityEngine;

namespace Reptile
{
	public class CameraModeWallrun : CameraMode
	{
		private WallrunLineAbility wallrunAbility;

		private float startFade;

		public CameraModeWallrun(WallrunLineAbility wallrunAbility)
		{
			this.wallrunAbility = wallrunAbility;
		}

		public override void Activate(Player player, CameraMode modePrev)
		{
			base.Activate(player, modePrev);
			inputEnabled = true;
			startFade = 0f;
		}

		protected override void UpdatePos(int fixedFrameCounter)
		{
			startFade += base.dt * 2f;
			if (startFade > 1f)
			{
				startFade = 1f;
			}
			lookAtPos = LerpTo(lookAtPos, player.tf.position + wallrunAbility.wallrunFaceNormal * cam.wallrunPlayerCenterOffset + Vector3.up * cam.wallrunPlayerYOffset, 15f);
			float y = HandleCamInput(position).y;
			Vector3 target = lookAtPos + wallrunAbility.wallrunFaceNormal * (cam.wallrunAwayFromWall - cam.wallrunPlayerCenterOffset);
			position = LerpTo(position, target, cam.wallrunDragBehind);
			position.y = Mathf.Lerp(position.y, y, manualOrbitFactor * startFade);
			positionFinal = HandleObstructions(position);
			tilt = cam.wallrunTiltAngle * ((player.wallrunAbility.animSide == Side.RIGHT) ? 1f : (-1f));
		}
	}
}
