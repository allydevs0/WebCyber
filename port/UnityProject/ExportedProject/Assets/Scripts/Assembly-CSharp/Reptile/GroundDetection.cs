using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(CapsuleCollider))]
	public sealed class GroundDetection : BaseGroundDetection
	{
		private struct RaycastInfo
		{
			public Vector3 pos;

			public Vector3 dir;

			public bool hit;

			public RaycastHit hitInfo;

			public void DrawGizmos(Color col)
			{
				if (hit)
				{
					Gizmos.color = col;
					Vector3 vector = pos + dir * hitInfo.distance;
					Gizmos.DrawLine(pos, vector);
					Gizmos.DrawSphere(vector, 0.02f);
				}
				else
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(pos, pos + dir * 0.5f);
				}
			}
		}

		private Vector3 showExtraPoint;

		private Vector3 extraPointNormal;

		private Color extraPointColor;

		private RaycastInfo rayLeft;

		private RaycastInfo rayRight;

		private RaycastInfo rayFront;

		private RaycastInfo rayBack;

		internal const int NonStableLayerMask = 1040;

		internal static bool LayerIsNonStable(int layer)
		{
			if (layer != 10)
			{
				return layer == 4;
			}
			return true;
		}

		public override bool ComputeGroundHit(Vector3 position, Quaternion rotation, ref GroundHit groundHitInfo, float distance = float.PositiveInfinity)
		{
			Vector3 vector = rotation * Vector3.down;
			showExtraPoint = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			Vector3 vector3 = Vector3.zero;
			bool? flag = null;
			bool flag2 = base.player.playerType == PlayerType.HUMAN;
			if (flag2)
			{
				rayLeft = GetRaycastInfo(position - base.transform.right * 0.2f, vector, distance, 1f);
				rayRight = GetRaycastInfo(position + base.transform.right * 0.2f, vector, distance, 1f);
				rayFront = GetRaycastInfo(position + base.transform.forward * 0.4f, vector, distance, 1f);
				rayBack = GetRaycastInfo(position - base.transform.forward * 0.4f, vector, distance, 1f);
				if (rayLeft.hit && rayRight.hit && rayFront.hit && rayBack.hit)
				{
					Vector3 point = rayRight.hitInfo.point;
					Vector3 point2 = rayLeft.hitInfo.point;
					Vector3 point3 = rayFront.hitInfo.point;
					vector2 = Vector3.Cross(rhs: point3 - rayBack.hitInfo.point, lhs: point - point2).normalized;
					if (vector2.y < 0f)
					{
						vector2 = -vector2;
					}
					if (rayRight.hitInfo.normal.y > 0.99f && rayLeft.hitInfo.normal.y > 0.99f)
					{
						vector3 = Vector3.up;
					}
				}
			}
			bool num = !Raycast(position, vector, out var hitInfo, distance);
			bool flag3 = !BottomSphereCast(position, rotation, out var hitInfo2, distance);
			if (num)
			{
				if (flag3)
				{
					return false;
				}
				if (hitInfo2.collider.gameObject.layer == 23)
				{
					groundHitInfo.SetFrom(hitInfo2);
					showExtraPoint = hitInfo2.point;
					extraPointColor = Color.cyan;
					extraPointNormal = hitInfo2.normal;
				}
				else if (Vector3.Angle(hitInfo2.normal, Vector3.up) > base.groundLimit || LayerIsNonStable(hitInfo2.collider.gameObject.layer))
				{
					groundHitInfo.SetFrom(hitInfo2);
					showExtraPoint = hitInfo2.point;
					extraPointColor = Color.magenta;
					extraPointNormal = hitInfo2.normal;
				}
				else if (hitInfo2.point.y < position.y)
				{
					groundHitInfo.SetFrom(hitInfo2);
					groundHitInfo.groundPoint = new Vector3(position.x, hitInfo2.point.y, position.z);
					vector3 = hitInfo2.normal;
					vector2 = Vector3.up;
					groundHitInfo.groundDistance = 0.001f;
					flag = true;
				}
				else
				{
					if (hitInfo2.point.y < position.y && new Vector2(position.x - hitInfo2.point.x, position.z - hitInfo2.point.z).sqrMagnitude > 0.01f)
					{
						return false;
					}
					showExtraPoint = hitInfo2.point;
					extraPointColor = Color.gray;
					extraPointNormal = hitInfo2.normal;
					groundHitInfo.SetFrom(hitInfo2);
					groundHitInfo.groundPoint = new Vector3(position.x, hitInfo2.point.y, position.z);
					vector2 = Vector3.up;
				}
			}
			else
			{
				groundHitInfo.SetFrom(hitInfo);
				if (groundHitInfo.groundCollider.gameObject.layer != 23 && flag2 && rayFront.hit)
				{
					RaycastHit hitInfo3 = rayFront.hitInfo;
					float num2 = hitInfo3.point.y - hitInfo.point.y;
					extraPointColor = Color.yellow;
					if (num2 > 0.02f)
					{
						if (Vector3.Angle(hitInfo3.normal, Vector3.up) > base.groundLimit)
						{
							flag = true;
							vector2 = hitInfo.normal;
							extraPointColor = Color.red;
						}
						else if (num2 > base.stepOffset && rayFront.hitInfo.normal.y > 0.999f)
						{
							flag = true;
							vector2 = hitInfo.normal;
							extraPointColor = Color.black;
						}
						else if (LayerIsNonStable(hitInfo3.collider.gameObject.layer))
						{
							flag = true;
							groundHitInfo.groundDistance = 0.2f;
							vector2 = hitInfo.normal;
							extraPointColor = Color.magenta;
						}
						else if (num2 <= base.stepOffset && hitInfo.normal.y > 0.999f && hitInfo3.normal.y > 0.999f)
						{
							extraPointColor = Color.blue;
							float num3 = (groundHitInfo.groundDistance = hitInfo3.point.y - position.y);
							groundHitInfo.stepHeight = num3;
							groundHitInfo.isOnStep = true;
							flag = true;
						}
					}
				}
			}
			float num5 = hitInfo2.point.y - hitInfo.point.y;
			if (num5 > 0.05f && num5 < base.stepOffset && hitInfo.normal.y > 0.99f && !flag.HasValue)
			{
				flag = false;
			}
			if (vector2 != Vector3.zero)
			{
				groundHitInfo.groundNormal = vector2;
			}
			groundHitInfo.groundNormalVisual = ((vector3 == Vector3.zero) ? groundHitInfo.groundNormal : vector3);
			if (groundHitInfo.groundNormalVisual.y > 0.999f && !flag.HasValue)
			{
				flag = true;
			}
			if (!flag.HasValue || !flag.Value)
			{
				groundHitInfo.groundDistance = 0f;
			}
			groundHitInfo.isOnGround = true;
			int layer = groundHitInfo.groundCollider.gameObject.layer;
			groundHitInfo.isValidGround = !LayerIsNonStable(layer) && (Vector3.Angle(groundHitInfo.groundNormalVisual, Vector3.up) < base.groundLimit || layer == 23);
			return true;
		}

		private string HitStr(RaycastHit hit, Vector3 pos)
		{
			return $"dY {hit.point.y - pos.y:0.00}  NY {hit.normal.y:0.000}";
		}

		public override bool FindGround(Vector3 direction, out RaycastHit hitInfo, float distance = float.PositiveInfinity, float backstepDistance = 0.05f)
		{
			float radius = base.capsuleCollider.radius;
			float num = Mathf.Max(0f, base.capsuleCollider.height * 0.5f - radius);
			Vector3 position = base.capsuleCollider.center - Vector3.up * num;
			Vector3 origin = base.transform.TransformPoint(position);
			if (!SphereCast(origin, radius, direction, out hitInfo, distance, backstepDistance) || Vector3.Angle(hitInfo.normal, Vector3.up) >= 89f)
			{
				return false;
			}
			return true;
		}

		protected override void DrawGizmos()
		{
		}

		private RaycastInfo GetRaycastInfo(Vector3 pos, Vector3 dir, float dist, float backstep)
		{
			RaycastInfo result = default;
			result.pos = pos;
			result.dir = dir;
			result.hit = Raycast(pos, dir, out result.hitInfo, dist, backstep);
			return result;
		}
	}
}
