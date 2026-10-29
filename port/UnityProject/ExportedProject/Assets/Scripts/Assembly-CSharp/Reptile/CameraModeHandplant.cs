using UnityEngine;

namespace Reptile
{
	public class CameraModeHandplant : CameraMode
	{
		private HandplantAbility handPlantAbility;

		private bool goToDefault;

		public CameraModeHandplant(HandplantAbility handPlantAbility)
		{
			this.handPlantAbility = handPlantAbility;
		}

		public override void Activate(Player player, CameraMode modePrev)
		{
			base.Activate(player, modePrev);
			inputEnabled = true;
			goToDefault = true;
		}

		protected override void UpdatePos(int fixedFrameCounter)
		{
			HandleLookAt(fixedFrameCounter);
			bool flag = cam.orbitInput != Vector2.zero;
			if (flag)
			{
				goToDefault = false;
			}
			position = HandleCamInput(position, new Vector2(player.moveInputPlain.x, 0f));
			position = LerpTo(position, lookAtPos + (position - lookAtPos).normalized * cam.handplantCamDist, 2f);
			if (handPlantAbility.screwSpinTimer > 0f)
			{
				manualOrbitFactor = Mathf.Clamp01(lastOrbitTimer);
				position.y = LerpTo(position.y, lookAtPos.y + cam.handplantHeightOffsetDefault, 6f * manualOrbitFactor);
			}
			else if (goToDefault || (player.moveInputPlain.x != 0f && !flag))
			{
				position.y = LerpTo(position.y, lookAtPos.y + cam.handplantHeightOffsetDefault, 6f * manualOrbitFactor);
			}
			positionFinal = HandleObstructions(position);
		}
	}
}
