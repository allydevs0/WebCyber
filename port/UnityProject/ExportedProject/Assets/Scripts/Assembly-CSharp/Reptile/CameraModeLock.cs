using System;
using UnityEngine;

namespace Reptile
{
	public class CameraModeLock : CameraMode
	{
		private Transform tfLock;

		private Func<bool> funcValid;

		private bool canSwitchToOtherEnemies;

		private static bool holdingCamera;

		private static float timeLastInput;

		private const float maxFindAngle = 90f;

		private const float maxDist = 30f;

		private const int sightLineMask = 2106385;

		public CameraModeLock(Transform tfLock, Func<bool> funcValid = null, bool canSwitchToOtherEnemies = false)
		{
			this.tfLock = tfLock;
			this.funcValid = funcValid;
			this.canSwitchToOtherEnemies = canSwitchToOtherEnemies;
			inputEnabled = true;
		}

		protected override void UpdatePos(int fixedFrameCounter)
		{
			if (tfLock == null || (funcValid != null && !funcValid()))
			{
				tfLock = null;
				cam.UnsetCamLock();
				return;
			}
			if (canSwitchToOtherEnemies)
			{
				float num = 0.5f;
				float num2 = (cam.mouseForOrbit ? 5f : 1f);
				Vector2 orbitInput = cam.orbitInput;
				float num3 = Mathf.Max(Mathf.Abs(orbitInput.x), Mathf.Abs(orbitInput.y));
				if (num3 < num)
				{
					holdingCamera = false;
				}
				else if (num3 > num2 && (timeLastInput < Time.realtimeSinceStartup - 0.5f || timeLastInput > Time.realtimeSinceStartup) && !holdingCamera)
				{
					holdingCamera = true;
					timeLastInput = Time.realtimeSinceStartup;
					BasicEnemy basicEnemy = FindEnemyFromCur(orbitInput);
					if (basicEnemy != null)
					{
						cam.SetCamLock(basicEnemy);
					}
				}
			}
			lookAtPos = tfLock.position;
			targetLookAtY = lookAtPos.y;
			Vector3 vector = player.tf.position;
			Vector3 vector2 = vector - lookAtPos;
			float magnitude = vector2.magnitude;
			position = lookAtPos + vector2 / magnitude * (magnitude + 3f);
			position.y = vector.y + 2.5f;
			positionFinal = HandleObstructions(position, vector + Vector3.up * 1.5f);
		}

		public static BasicEnemy FindEnemy(GameplayCamera cam)
		{
			Transform realTf = cam.realTf;
			BasicEnemy result = null;
			BasicEnemy[] array = UnityEngine.Object.FindObjectsOfType<BasicEnemy>();
			if (array.Length != 0)
			{
				float num = 90f;
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 vector = array[i].transform.position + Vector3.up * 1.5f - realTf.position;
					float num2 = Vector3.Angle(realTf.forward, vector);
					if (!(num2 > num))
					{
						float sqrMagnitude = vector.sqrMagnitude;
						if (!(sqrMagnitude > 900f) && !Physics.Raycast(realTf.position, vector, Mathf.Sqrt(sqrMagnitude), 2106385))
						{
							num = num2;
							result = array[i];
						}
					}
				}
			}
			return result;
		}

		private BasicEnemy FindEnemyFromCur(Vector2 steer)
		{
			Transform realTf = cam.realTf;
			BasicEnemy result = null;
			BasicEnemy[] array = UnityEngine.Object.FindObjectsOfType<BasicEnemy>();
			if (array.Length != 0)
			{
				float num = float.MaxValue;
				Vector2 vector = cam.cam.WorldToScreenPoint(tfLock.position);
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 vector2 = array[i].transform.position + Vector3.up * 1.5f - realTf.position;
					if (array[i].transform == tfLock || Vector3.Angle(realTf.forward, vector2) > 90f)
					{
						continue;
					}
					float sqrMagnitude = vector2.sqrMagnitude;
					if (sqrMagnitude > 900f)
					{
						continue;
					}
					Vector2 lhs = (Vector2)cam.cam.WorldToScreenPoint(array[i].transform.position) - vector;
					if (!(Vector2.Dot(lhs, steer) < 0f))
					{
						float sqrMagnitude2 = lhs.sqrMagnitude;
						if (!(sqrMagnitude2 > num) && !Physics.Raycast(realTf.position, vector2, Mathf.Sqrt(sqrMagnitude), 2106385))
						{
							num = sqrMagnitude2;
							result = array[i];
						}
					}
				}
			}
			return result;
		}
	}
}
