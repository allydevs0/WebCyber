using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class TurretEnemy : BasicEnemy
	{
		private enum TurretState
		{
			NORMAL = 0,
			HITPAUSE = 1,
			KNOCKBACK_UP = 2,
			KNOCKBACK_STAY = 3,
			DOWN = 4,
			RISING = 5,
			LOWERING = 6
		}

		private enum HatchState
		{
			IN = 0,
			EXTENDING = 1,
			OUT = 2,
			RETRACTING = 3
		}

		private enum ClawState
		{
			CLOSED = 0,
			OPENED = 1,
			ATTACKING = 2
		}

		private TurretState state = TurretState.DOWN;

		private HatchState hatchState;

		private Vector3 hitDir;

		private Vector3 cockpitHiddenPos;

		private Vector3 pistonHiddenPos;

		private Vector3 cockpitShowPos;

		private Vector3 hatchRHiddenPos;

		private Vector3 hatchLHiddenPos;

		private Vector3 hatchRExtendedPos;

		private Vector3 hatchLExtendedPos;

		private float extensionHeight = 2.5f;

		private float hitExtent = 2.5f;

		private float rotSpeed = 120f;

		private float pistonVertSpeed = 5f;

		private float headVertRotSpeed = 100f;

		private float stateTimer;

		private float hatchDistance = 0.6625f;

		private bool isPlayingCuffLoop;

		public Transform reflectTarget;

		[SerializeField]
		private GameObject missilePrefab;

		[SerializeField]
		private Transform barrelL;

		[SerializeField]
		private Transform barrelR;

		[SerializeField]
		private Transform hatchR;

		[SerializeField]
		private Transform hatchL;

		[SerializeField]
		private Transform head;

		[SerializeField]
		private Transform piston;

		[SerializeField]
		private Transform cockpit;

		[SerializeField]
		private Animation pilot;

		[SerializeField]
		private ParticleSystem brokenVFX;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private CuffMissileLauncher missileLauncherLeft;

		[SerializeField]
		private CuffMissileLauncher missileLauncherRight;

		[SerializeField]
		private EnemyGraffitiSpotDamager graffitiSpotDamager;

		[SerializeField]
		private GameObject clawTemp;

		[SerializeField]
		private GameObject hitBox;

		[SerializeField]
		private Collider bounceCollider;

		private bool dead;

		public bool fireBothBarrels = true;

		internal List<Transform> activeMissiles = new List<Transform>();

		private Quaternion startRot;

		private Quaternion targetPistonRotation;

		private Rigidbody headBody;

		private Rigidbody pistonBody;

		private Vector3 pistonStartPos;

		private Vector3 headOriginLocalPos;

		private List<IPoolable> poolables = new List<IPoolable>();

		public float agroMinRange = 10f;

		private float agroRangeTimer;

		private ClawState clawState;

		private float attackCooldown;

		protected override void Setup()
		{
			base.Setup();
			pistonBody = piston.GetComponent<Rigidbody>();
			headBody = head.GetComponent<Rigidbody>();
			hatchRExtendedPos = hatchR.localPosition;
			hatchLExtendedPos = hatchL.localPosition;
			hatchRHiddenPos = hatchR.localPosition + new Vector3(hatchDistance, 0f, 0f);
			hatchLHiddenPos = hatchL.localPosition - new Vector3(hatchDistance, 0f, 0f);
			cockpitShowPos = cockpit.localPosition;
			cockpitHiddenPos = cockpit.localPosition + Vector3.down * 1.45f;
			pistonStartPos = piston.localPosition;
			pistonHiddenPos = piston.localPosition + Vector3.down * 2.25f;
			headOriginLocalPos = head.localPosition;
			startRot = piston.rotation;
			graffitiSpotDamager.Init(this);
			missileLauncherLeft.Init(this, positioningInfo);
			missileLauncherRight.Init(this, positioningInfo);
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
		}

		protected override void UpdateSpawning()
		{
			base.UpdateSpawning();
			SetBasicState(BasicEnemyState.ACTIVE);
		}

		protected override void EnteringActiveState()
		{
			base.EnteringActiveState();
			SetState(TurretState.DOWN);
			HP = fullHP;
			dead = false;
			graffitiSpotDamager.ResetGraffitiSpots();
			brokenVFX.Stop();
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			if (player.IsBusyWithSequence())
			{
				return;
			}
			stateTimer += Core.dt;
			missileLauncherLeft.UpdateFirearm();
			missileLauncherRight.UpdateFirearm();
			piston.position += velocity * Core.dt;
			Quaternion to = head.localRotation;
			if (!dead)
			{
				targetPistonRotation = startRot;
				to = Quaternion.identity;
			}
			switch (state)
			{
			case TurretState.DOWN:
				if (positioningInfo.SqrDistToPlayer > agroMinRange * agroMinRange && WorldHandler.instance.IsKindaInScreen(piston.position))
				{
					SetState(TurretState.RISING);
				}
				break;
			case TurretState.RISING:
				velocity = Vector3.zero;
				piston.localPosition += Vector3.up * pistonVertSpeed * Core.dt;
				if (piston.localPosition.y >= pistonStartPos.y)
				{
					piston.localPosition = pistonStartPos;
					SetState(TurretState.NORMAL);
				}
				break;
			case TurretState.LOWERING:
				targetPistonRotation = startRot;
				if (hatchState != HatchState.IN)
				{
					RetractHatches();
				}
				else if (piston.rotation == targetPistonRotation)
				{
					piston.localPosition -= Vector3.up * pistonVertSpeed * Core.dt;
					velocity = Vector3.zero;
					if (piston.localPosition.y <= pistonHiddenPos.y)
					{
						SetState(TurretState.DOWN);
					}
				}
				break;
			case TurretState.NORMAL:
				targetPistonRotation = Quaternion.LookRotation(positioningInfo.FlatDirToPlayer);
				PistonGravity();
				if (cockpit.localPosition.y > cockpitHiddenPos.y)
				{
					cockpit.localPosition -= Vector3.up * 3f * Core.dt;
					if (cockpit.localPosition.y < cockpitHiddenPos.y)
					{
						cockpit.localPosition = cockpitHiddenPos;
					}
				}
				if (!dead)
				{
					attackCooldown += Core.dt;
					bool flag = positioningInfo.SqrDistToPlayer > agroMinRange * agroMinRange;
					if (positioningInfo.SqrDistToPlayer < 100f)
					{
						agroRangeTimer += Core.dt;
						if (clawState == ClawState.CLOSED && attackCooldown > 5f)
						{
							clawState = ClawState.OPENED;
							clawTemp.GetComponent<Animator>().Play("turretClawOpen");
							agroRangeTimer = 0f;
						}
						if (!flag && clawState == ClawState.OPENED && attackCooldown > 5f && agroRangeTimer > 0.5f)
						{
							hitBox.SetActive(value: true);
							clawTemp.GetComponent<Animator>().Play("turretClawAttack");
							clawState = ClawState.ATTACKING;
							attackCooldown = 0f;
						}
						if (clawState == ClawState.ATTACKING && attackCooldown > 2f && player.ability != player.caughtAbility)
						{
							clawTemp.GetComponent<Animator>().Play("turretClawOpened");
							clawState = ClawState.OPENED;
						}
					}
					else if (agroRangeTimer > 0f && clawState != ClawState.CLOSED)
					{
						clawTemp.GetComponent<Animator>().Play("turretClawClose");
						clawState = ClawState.CLOSED;
						agroRangeTimer = -0.2f;
					}
					if ((!(Vector3.Dot(piston.rotation * Vector3.forward, positioningInfo.FlatDirToPlayer) < 0.8f) || hatchState == HatchState.OUT) && hatchState != HatchState.OUT)
					{
						ExtendHatches();
						stateTimer = 0f;
					}
					if (hatchState == HatchState.OUT)
					{
						bool tilted = false;
						if (positioningInfo.SqrDistToPlayer > 225f)
						{
							float num = Mathf.Max(0f, Vector3.Angle(positioningInfo.FlatDirToPlayer, positioningInfo.DirToPlayer));
							if (num > 30f)
							{
								tilted = true;
								to = Quaternion.Euler(0f - num, 0f, 0f);
							}
						}
						if (flag && stateTimer > 0.5f && missileLauncherLeft.CanShoot && missileLauncherRight.CanShoot && Vector3.Dot(piston.rotation * Vector3.forward, positioningInfo.FlatDirToPlayer) > 0.8f)
						{
							RequestAttack(2f);
							if (receivedAttackPermission)
							{
								Fire(positioningInfo.DirToPlayer, Mathf.Sqrt(positioningInfo.SqrDistToPlayer), tilted);
								ScheduleAttackRelease(2f);
							}
						}
					}
					head.localRotation = Quaternion.RotateTowards(head.localRotation, to, headVertRotSpeed * Core.dt);
				}
				else
				{
					RetractHatches();
				}
				break;
			case TurretState.HITPAUSE:
				if (!IsDead())
				{
					targetPistonRotation = Quaternion.LookRotation(positioningInfo.FlatDirToPlayer);
					WorldHandler.instance.AddCombatBlocker(4f);
				}
				if (stateTimer > player.hitBounceAbility.hitpauseDuration)
				{
					SetState(TurretState.KNOCKBACK_UP);
				}
				break;
			case TurretState.KNOCKBACK_UP:
				if (!dead)
				{
					targetPistonRotation = Quaternion.LookRotation(positioningInfo.FlatDirToPlayer);
					head.localRotation = Quaternion.RotateTowards(head.localRotation, Quaternion.identity, 250f * Core.dt);
				}
				if (piston.localPosition.y > pistonStartPos.y + extensionHeight)
				{
					SetState(TurretState.KNOCKBACK_STAY);
				}
				break;
			case TurretState.KNOCKBACK_STAY:
				if (!IsDead())
				{
					targetPistonRotation = Quaternion.LookRotation(positioningInfo.FlatDirToPlayer);
					cockpit.localPosition += Vector3.up * 10f * Core.dt;
					if (cockpit.localPosition.y > cockpitShowPos.y)
					{
						cockpit.localPosition = cockpitShowPos;
						graffitiSpotDamager.SetGraffitiSpotsActive(active: true);
					}
				}
				else if (stateTimer > 1.5f)
				{
					cockpit.localPosition -= Vector3.up * 2f * Core.dt;
					if (cockpit.localPosition.y < cockpitHiddenPos.y)
					{
						cockpit.localPosition = cockpitHiddenPos;
					}
				}
				if (stateTimer < 0.2f)
				{
					head.localPosition = headOriginLocalPos + Random.insideUnitSphere * 0.05f;
				}
				if (stateTimer > 3f)
				{
					SetState(TurretState.NORMAL);
				}
				break;
			}
			if (!dead && player.ability != player.caughtAbility)
			{
				piston.rotation = Quaternion.RotateTowards(piston.rotation, targetPistonRotation, rotSpeed * Core.dt);
			}
		}

		private void PistonGravity()
		{
			if (piston.localPosition.y < pistonStartPos.y)
			{
				velocity = Vector3.zero;
				piston.localPosition = new Vector3(piston.localPosition.x, pistonStartPos.y, piston.localPosition.z);
			}
			else
			{
				velocity.y = Mathf.Max(-30f, velocity.y - 10f * Core.dt);
			}
		}

		private void SetState(TurretState setState)
		{
			TurretState turretState = state;
			state = setState;
			stateTimer = 0f;
			head.localPosition = headOriginLocalPos;
			switch (state)
			{
			case TurretState.DOWN:
				cockpit.localPosition = cockpitHiddenPos;
				piston.localPosition = pistonHiddenPos;
				hatchR.localPosition = hatchRHiddenPos;
				hatchL.localPosition = hatchLHiddenPos;
				piston.rotation = startRot;
				velocity = Vector3.zero;
				break;
			case TurretState.RISING:
				piston.rotation = startRot;
				break;
			case TurretState.LOWERING:
				ClearMissiles();
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TurretDown, audioSource);
				break;
			case TurretState.NORMAL:
				agroRangeTimer = 0f;
				if (!dead)
				{
					graffitiSpotDamager.SetGraffitiSpotsActive(active: false);
				}
				break;
			case TurretState.HITPAUSE:
				if (turretState != TurretState.HITPAUSE)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.CockpitGetHit, audioSource);
					clawTemp.GetComponent<Animator>().Play("turretClawClose");
					clawState = ClawState.CLOSED;
				}
				break;
			case TurretState.KNOCKBACK_UP:
			{
				velocity = Vector3.up * 12f;
				Vector3 vector = player.transform.position - piston.position;
				vector.y = 0f;
				piston.rotation = (targetPistonRotation = Quaternion.LookRotation(vector.normalized));
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TurretRaise, audioSource);
				break;
			}
			case TurretState.KNOCKBACK_STAY:
				piston.localPosition = pistonStartPos + Vector3.up * extensionHeight;
				velocity = Vector3.zero;
				break;
			}
		}

		private void Dead()
		{
			if (!dead)
			{
				if (!spawnedInEncounter)
				{
					WantedManager.instance.CrimePerpetrated(WantedManager.Crime.OFFICER_DOWN);
				}
				brokenVFX.Play();
				ClearMissiles();
			}
			dead = true;
			WorldHandler.instance.ReleaseAttack(this);
		}

		public override bool IsDead()
		{
			return dead;
		}

		private void ClearMissiles()
		{
			missileLauncherLeft.ClearMissiles();
			missileLauncherRight.ClearMissiles();
		}

		private void Knockback(Vector3 fromDirection)
		{
			hitDir = fromDirection;
			velocity = Vector3.zero;
			SetState(TurretState.HITPAUSE);
		}

		public void PlayerCaught()
		{
		}

		public void PlayerEscaped()
		{
			Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.CockpitGetHit, audioSource);
			clawState = ClawState.CLOSED;
			clawTemp.GetComponent<Animator>().Play("turretClawClose");
			attackCooldown = 0f;
		}

		private void ExtendHatches()
		{
			if (hatchState != HatchState.OUT)
			{
				if (hatchState == HatchState.IN)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, audioSource);
				}
				hatchState = HatchState.EXTENDING;
				if (hatchR.localPosition.x > hatchRExtendedPos.x)
				{
					hatchR.position -= hatchR.right * hatchDistance * Core.dt * 3f;
					hatchL.position += hatchL.right * hatchDistance * Core.dt * 3f;
				}
				else
				{
					hatchState = HatchState.OUT;
				}
			}
		}

		private void RetractHatches()
		{
			if (hatchState != HatchState.IN)
			{
				if (hatchState == HatchState.OUT)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, audioSource);
				}
				hatchState = HatchState.RETRACTING;
				if (hatchR.localPosition.x < hatchRExtendedPos.x + hatchDistance)
				{
					hatchR.position += hatchR.right * hatchDistance * Core.dt * 3f;
					hatchL.position -= hatchL.right * hatchDistance * Core.dt * 3f;
				}
				else
				{
					hatchState = HatchState.IN;
					ClearMissiles();
				}
			}
		}

		public override void OnTriggerStay(Collider other)
		{
			base.OnTriggerStay(other);
			if (other.gameObject.layer != 18)
			{
				return;
			}
			Vector3 vector = player.tf.position - piston.position;
			vector.y = 0f;
			Vector3 normalized = vector.normalized;
			Knockback(normalized);
			ClearMissiles();
			Vector3 position = player.tf.position;
			position = piston.transform.position + normalized * hitExtent;
			position.y = player.tf.position.y;
			HitBounceAbility.BounceType bounceType = HitBounceAbility.BounceType.TURRET_BOUNCE_FROM_AIR_GRAF;
			if (player.IsGrounded())
			{
				bounceType = HitBounceAbility.BounceType.TURRET_BOUNCE_FROM_GROUND_GRAF;
			}
			else if (piston.localPosition.y > pistonStartPos.y + 1f)
			{
				bounceType = HitBounceAbility.BounceType.PLAIN_BOUNCE;
			}
			if (dead)
			{
				switch (bounceType)
				{
				case HitBounceAbility.BounceType.TURRET_BOUNCE_FROM_GROUND_GRAF:
					bounceType = HitBounceAbility.BounceType.TURRET_BOUNCE_FROM_GROUND;
					break;
				case HitBounceAbility.BounceType.TURRET_BOUNCE_FROM_AIR_GRAF:
					bounceType = HitBounceAbility.BounceType.TURRET_BOUNCE_FROM_AIR;
					break;
				}
			}
			if (other.gameObject.GetComponentInParent<Player>() == player)
			{
				player.hitBounceAbility.TurretBounce(base.gameObject, bounceCollider, bounceType);
			}
		}

		private void Fire(Vector3 dirToPlayer, float distToPlayer, bool tilted)
		{
			Quaternion quaternion = Quaternion.LookRotation(dirToPlayer);
			if (!tilted && distToPlayer > 18f)
			{
				quaternion = Quaternion.Euler(quaternion.eulerAngles.x - 40f, quaternion.eulerAngles.y, quaternion.eulerAngles.z);
			}
			missileLauncherLeft.Shoot(quaternion * Vector3.forward, 0f);
			missileLauncherRight.Shoot(quaternion * Vector3.forward, 0f);
		}

		protected override void Die()
		{
			base.Die();
			if (graffitiSpotDamager.GSpots[0].lastPlayerToPaint == player)
			{
				WorldHandler.instance.AddCombatBlocker(5f);
				WantedManager.instance.CrimePerpetrated(WantedManager.Crime.OFFICER_DOWN);
				player.tf.position = graffitiSpotDamager.GSpots[0].GetGraffitiSpotPlayerPos();
				player.SetVelocity(player.dir * 2f + Vector3.up * 2.5f);
			}
			Dead();
		}

		public override void OnPlayerRevive()
		{
			base.OnPlayerRevive();
		}

		public override void StandDown(bool alsoStandDownFromBattle = false)
		{
			base.StandDown();
			SetState(TurretState.DOWN);
			ClearMissiles();
			hatchState = HatchState.IN;
		}

		public override bool ShouldBeReplaced()
		{
			if (!IsDead())
			{
				return base.ShouldBeReplaced();
			}
			return false;
		}
	}
}
