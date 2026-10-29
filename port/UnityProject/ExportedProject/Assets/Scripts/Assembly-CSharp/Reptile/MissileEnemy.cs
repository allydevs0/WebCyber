using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class MissileEnemy : MonoBehaviour
	{
		public enum MissileState
		{
			ACTIVE = 0,
			REFLECTED = 1,
			OUT_OF_CONTROL = 2,
			JUNKED = 3,
			SPECIAL_SKATEBOARD = 4
		}

		internal TurretEnemy owner;

		private Player player;

		private GameObject effect;

		private Rigidbody rBody;

		private bool hadSelectedReflectTarget;

		private Transform reflectTarget;

		private Vector3 targetPos;

		private Quaternion targetRotation;

		private Vector3 outOfControlDeviate;

		public float rotSpeed;

		public float speed;

		public GameObject explosionEffect;

		public GameObject linePrefab;

		private LineRenderer line;

		private float stateTimer;

		private float linePointInterval = 0.25f;

		private float linePointTimer;

		private List<Vector3> chainPosList;

		private MissileState state;

		private void Start()
		{
			player = WorldHandler.instance.GetCurrentPlayer();
			effect = base.transform.FindRecursive("boostEffect").gameObject;
			rBody = GetComponent<Rigidbody>();
			Core.OnUpdate += UpdateMissileEnemy;
			chainPosList = new List<Vector3>(1) { base.transform.position };
			line = Object.Instantiate(linePrefab).GetComponent<LineRenderer>();
			SetState(MissileState.ACTIVE);
		}

		private void UpdateMissileEnemy()
		{
			stateTimer += Core.dt;
			linePointTimer += Core.dt;
			if (linePointTimer > linePointInterval)
			{
				linePointTimer = 0f;
				chainPosList.Add(base.transform.position);
				line.positionCount = chainPosList.Count;
				line.SetPositions(chainPosList.ToArray());
			}
			switch (state)
			{
			case MissileState.ACTIVE:
				targetPos = player.capsuleCenter;
				MoveToTarget();
				break;
			case MissileState.OUT_OF_CONTROL:
				targetPos = base.transform.position + base.transform.rotation * outOfControlDeviate;
				MoveToTarget();
				speed += 5f * Core.dt;
				if (speed > 40f)
				{
					speed = 40f;
				}
				if (stateTimer > 15f)
				{
					Explode();
				}
				break;
			case MissileState.REFLECTED:
				base.transform.GetChild(0).localRotation *= Quaternion.Euler(2500f * Core.dt, 0f, 0f);
				reflectTarget = GetClosestActiveMissileOrOwnerInDirection();
				targetPos = reflectTarget.position;
				MoveToTarget();
				if (stateTimer > 15f)
				{
					Explode();
				}
				break;
			case MissileState.JUNKED:
				if (stateTimer > 1f)
				{
					base.gameObject.layer = 21;
				}
				if (stateTimer > 40f)
				{
					Object.Destroy(base.gameObject);
				}
				break;
			case MissileState.SPECIAL_SKATEBOARD:
				break;
			}
		}

		public void MoveToTarget()
		{
			base.transform.position += base.transform.forward * speed * Core.dt;
			Vector3 normalized = (targetPos - base.transform.position).normalized;
			targetRotation = Quaternion.LookRotation(normalized);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, rotSpeed * Core.dt);
		}

		public void SetState(MissileState setState)
		{
			stateTimer = 0f;
			state = setState;
			if (setState != MissileState.ACTIVE)
			{
				owner.activeMissiles.Remove(base.transform);
			}
			switch (state)
			{
			case MissileState.ACTIVE:
				ActivatePhysics(set: false);
				effect.SetActive(value: true);
				GetComponent<BoxCollider>().enabled = true;
				break;
			case MissileState.OUT_OF_CONTROL:
				rotSpeed *= 2f;
				ActivatePhysics(set: false);
				effect.SetActive(value: true);
				GetComponent<BoxCollider>().enabled = true;
				outOfControlDeviate = Random.insideUnitSphere;
				if (Vector3.Dot(outOfControlDeviate, base.transform.forward) > 0f)
				{
					outOfControlDeviate *= -1f;
				}
				break;
			case MissileState.REFLECTED:
				ActivatePhysics(set: false);
				base.transform.rotation = Quaternion.LookRotation((owner.reflectTarget.position - base.transform.position).normalized);
				speed *= 2f;
				rotSpeed *= 2f;
				effect.SetActive(value: false);
				stateTimer = 0f;
				GetComponent<BoxCollider>().enabled = true;
				break;
			case MissileState.JUNKED:
			{
				ActivatePhysics(set: true);
				effect.SetActive(value: false);
				float num = 40f;
				rBody.maxAngularVelocity = num;
				rBody.AddTorque(Random.rotation * Vector3.forward * num, ForceMode.Impulse);
				GetComponent<BoxCollider>().enabled = true;
				break;
			}
			case MissileState.SPECIAL_SKATEBOARD:
				ActivatePhysics(set: false);
				effect.SetActive(value: true);
				GetComponent<BoxCollider>().enabled = false;
				break;
			}
		}

		private void ActivatePhysics(bool set)
		{
			GetComponent<Junk>().enabled = set;
			GetComponent<BoxCollider>().isTrigger = !set;
			rBody.isKinematic = !set;
			rBody.useGravity = set;
			rBody.angularVelocity = Vector3.zero;
			rBody.velocity = Vector3.zero;
			base.gameObject.layer = (set ? 29 : 28);
		}

		public void Hit(Vector3 vel, bool giveUpBounce)
		{
			base.transform.parent = null;
			Vector3 vector = owner.reflectTarget.position - base.transform.position;
			if (Vector3.Dot(vel.normalized, vector.normalized) > 0.8f && vector.sqrMagnitude < 3600f)
			{
				SetState(MissileState.REFLECTED);
			}
			else
			{
				JunkIt(vel, giveUpBounce);
			}
		}

		public void JunkIt(Vector3 vel, bool upBounce)
		{
			SetState(MissileState.JUNKED);
			if (upBounce)
			{
				vel.y = 6f;
			}
			else
			{
				vel.y = 0f;
			}
			rBody.velocity = vel;
		}

		private Transform GetReflectTarget()
		{
			if (owner.activeMissiles.Count == 0)
			{
				return owner.reflectTarget;
			}
			return GetClosestActiveMissileOrOwnerInDirection();
		}

		private Transform GetClosestActiveMissileOrOwnerInDirection()
		{
			float num = 0.7f;
			Vector3 position = base.transform.position;
			float num2 = (position - owner.reflectTarget.position).sqrMagnitude;
			Transform result = owner.reflectTarget;
			foreach (Transform activeMissile in owner.activeMissiles)
			{
				float sqrMagnitude = (position - activeMissile.position).sqrMagnitude;
				if (sqrMagnitude < num2 && Vector3.Dot((activeMissile.position - position).normalized, base.transform.forward) > num)
				{
					result = activeMissile;
					num2 = sqrMagnitude;
				}
			}
			return result;
		}

		public void OnCollisionEnter(Collision collision)
		{
			if (state == MissileState.JUNKED && collision.gameObject.layer == 9)
			{
				Player componentInParent = collision.gameObject.GetComponentInParent<Player>();
				if (componentInParent.moveStyle != MoveStyle.SPECIAL_SKATEBOARD && componentInParent.ability == componentInParent.slideAbility)
				{
					SetState(MissileState.SPECIAL_SKATEBOARD);
					componentInParent.StopCurrentAbility();
				}
			}
		}

		public void OnTriggerStay(Collider other)
		{
			if (state == MissileState.JUNKED)
			{
				if (other.gameObject.layer == 18)
				{
					if (other.name.Contains("spray"))
					{
						JunkIt(Vector3.zero, upBounce: true);
					}
					else if (state != MissileState.REFLECTED)
					{
						Hit(other.transform.parent.forward * 0.2f, giveUpBounce: true);
					}
				}
			}
			else if (other.gameObject.layer == 9)
			{
				if (state != MissileState.REFLECTED)
				{
					Explode();
				}
			}
			else if (other.transform.IsChildOf(owner.transform))
			{
				if (state == MissileState.REFLECTED)
				{
					Explode();
				}
			}
			else if (other.gameObject.layer == 18)
			{
				if (other.name.Contains("spray"))
				{
					JunkIt(Vector3.zero, upBounce: true);
				}
				else if (state != MissileState.REFLECTED)
				{
					Hit(other.transform.parent.forward * 0.2f, giveUpBounce: true);
				}
			}
			else if (other.gameObject.layer == 17)
			{
				Explode();
			}
			else if (other.gameObject.layer == 28)
			{
				MissileEnemy component = other.GetComponent<MissileEnemy>();
				if (component.state == MissileState.REFLECTED && state == MissileState.ACTIVE)
				{
					if (Random.value > 0.5f)
					{
						if (component.reflectTarget == base.transform)
						{
							component.reflectTarget = null;
						}
						SetState(MissileState.OUT_OF_CONTROL);
					}
					else
					{
						Explode();
					}
				}
				else
				{
					base.transform.position += (base.transform.position - other.transform.position).normalized * Core.dt;
				}
			}
			else if ((other.gameObject.layer == 0 || other.gameObject.layer == 21 || other.gameObject.layer == 13 || GroundDetection.LayerIsNonStable(other.gameObject.layer)) && state != MissileState.REFLECTED)
			{
				Explode();
			}
		}

		public void Explode()
		{
			Object.Destroy(line.gameObject);
			Object.Instantiate(explosionEffect, base.transform.position, Quaternion.identity);
			owner.activeMissiles.Remove(base.transform);
			Object.Destroy(base.gameObject);
		}

		public void OnDestroy()
		{
			owner.activeMissiles.Remove(base.transform);
			Core.OnUpdate -= UpdateMissileEnemy;
		}
	}
}
