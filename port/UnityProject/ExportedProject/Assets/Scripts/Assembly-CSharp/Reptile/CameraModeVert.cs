using UnityEngine;

namespace Reptile
{
	public class CameraModeVert : CameraMode
	{
		private VertAbility vertAbility;

		private bool goingUp;

		private bool orbiting;

		private float phasePrev;

		private float phaseOrig;

		private float phaseTarget;

		private float phaseOrigTime;

		private float phaseTargetTime;

		private OrbitVector lookStart;

		public CameraModeVert(VertAbility vertAbility)
		{
			this.vertAbility = vertAbility;
		}

		public override void Activate(Player player, CameraMode modePrev)
		{
			base.Activate(player, modePrev);
			inputEnabled = true;
			goingUp = true;
			orbiting = false;
		}

		protected override void UpdatePos(int fixedFrameCounter)
		{
			orbiting = orbiting || cam.orbitInput != Vector2.zero;
			if (orbiting)
			{
				HandleLookAt(fixedFrameCounter);
				position = HandleCamInput(position);
				position = HandleDragging(position);
			}
			else
			{
				float y = player.reallyMovedVelocity.y;
				float vertSpeedThreshold = cam.vertSpeedThreshold;
				if (goingUp && y < 0f - vertSpeedThreshold)
				{
					goingUp = false;
				}
				float num = ((!goingUp) ? 1f : ((!(y > vertSpeedThreshold)) ? ((1f - y / vertSpeedThreshold) * 0.5f) : (phaseTarget = 0f)));
				if (num != phasePrev)
				{
					SetPhase(num);
					phasePrev = num;
				}
				float t = Mathf.InverseLerp(phaseOrigTime, phaseTargetTime, Time.time);
				float num2 = Mathf.Lerp(phaseOrig, phaseTarget, t);
				HandleLookAt(fixedFrameCounter, player.visualTf.up * 1.5f * num2);
				if (num == 0f)
				{
					lookStart = OrbitVector.FromTo(position, lookAtPos);
					position = HandleDragging(position);
				}
				else
				{
					OrbitVector look = new OrbitVector
					{
						angleHor = Mathf.Atan2(0f - player.tf.forward.z, 0f - player.tf.forward.x) * 57.29578f,
						angleVert = cam.vertFallAngle,
						dist = cam.vertFallCamDist
					};
					float f = FadeInOut(num2);
					position = OrbitVector.Lerp(lookStart, look, f).GetPos(lookAtPos);
				}
			}
			positionFinal = HandleObstructions(position);
		}

		private float FadeInOut(float f)
		{
			return (3f - 2f * f) * f * f;
		}

		public void SetPhase(float phase)
		{
			phaseOrig = phaseTarget;
			phaseTarget = phase;
			phaseOrigTime = Time.time;
			phaseTargetTime = phaseOrigTime + Time.fixedDeltaTime;
		}
	}
}
