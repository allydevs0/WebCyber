using UnityEngine;

namespace Reptile
{
	public class CameraModeZone : CameraMode
	{
		private Vector3 curVelOffset = Vector3.zero;

		public CameraZone cameraZone;

		private float targetExtentPlaneLimited;

		private Vector3 rotateAroundGrindingViewCorrection;

		private Vector3 normalPlaneLimited;

		private Vector3 localPosPlaneLimited;

		private Vector3 basePos;

		public CameraModeZone(CameraZone cameraZone)
		{
			this.cameraZone = cameraZone;
		}

		public override void Activate(Player player, CameraMode modePrev)
		{
			base.Activate(player, modePrev);
			inputEnabled = false;
			basePos = position;
			if (cameraZone.cylinderPlaneLimited)
			{
				normalPlaneLimited = GetCylinderNormal();
			}
			else
			{
				normalPlaneLimited = cameraZone.GetPlaneLimitNormal();
			}
			targetExtentPlaneLimited = cameraZone.GetPlaneLimitExtent();
			targetExtentPlaneLimited += cam.dragDistanceMax;
			localPosPlaneLimited = Vector3.Project(position - cameraZone.box.transform.position, normalPlaneLimited);
		}

		private Vector3 GetCylinderNormal()
		{
			Vector3 vector = lookAtPos - cameraZone.box.transform.position;
			vector.y = 0f;
			return vector.normalized;
		}

		private Vector3 GetRotateAroundNormal()
		{
			Vector3 vector = cameraZone.box.transform.position - player.tf.position;
			vector.y = 0f;
			return vector.normalized;
		}

		private float GetRotateUpDownAngle()
		{
			Vector3 vector = cameraZone.box.transform.position - player.tf.position;
			vector.y = (0f - vector.y) / 20f;
			return vector.y;
		}

		protected override void UpdatePos(int fixedFrameCounter)
		{
			HandleLookAt(fixedFrameCounter);
			switch (cameraZone.cameraMode)
			{
			case CameraZoneMode.FIXED:
				position = LerpTo(position, cameraZone.fixedCameraPoint.position, 3f);
				break;
			case CameraZoneMode.PLANE_LIMITED:
				if (cameraZone.cylinderPlaneLimited)
				{
					normalPlaneLimited = GetCylinderNormal();
				}
				localPosPlaneLimited = LerpTo(localPosPlaneLimited, targetExtentPlaneLimited * normalPlaneLimited, 3f);
				basePos = lookAtPos + normalPlaneLimited;
				basePos.y = player.tf.position.y + cam.defaultCamHeight * CamHeightFactor();
				position = cameraZone.box.transform.position + localPosPlaneLimited + Vector3.ProjectOnPlane(basePos - cameraZone.box.transform.position, normalPlaneLimited);
				break;
			case CameraZoneMode.ROTATEAROUND:
			{
				normalPlaneLimited = GetRotateAroundNormal();
				Vector3 vector = Vector3.Lerp(curVelOffset, player.GetVelocity().normalized, Core.dt * 10f);
				if (player.IsGrinding())
				{
					rotateAroundGrindingViewCorrection = Vector3.up * (GetRotateUpDownAngle() + 1f);
				}
				else
				{
					rotateAroundGrindingViewCorrection = LerpTo(rotateAroundGrindingViewCorrection, Vector3.up, 3f);
				}
				position = player.tf.position - normalPlaneLimited * 6f + rotateAroundGrindingViewCorrection - Vector3.ProjectOnPlane(vector, normalPlaneLimited) * 3f;
				curVelOffset = vector;
				break;
			}
			case CameraZoneMode.FIXED_ANGLE:
				position = lookAtPos + Quaternion.AngleAxis(cameraZone.fixedAngle, Vector3.up) * cameraZone.box.transform.forward * cameraZone.fixedAngleDistance;
				break;
			}
			positionFinal = position;
			if (cameraZone.showAreaName != "")
			{
				player.ShowInformation(cameraZone.showAreaName, 0.5f);
			}
		}
	}
}
