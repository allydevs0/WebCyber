using UnityEngine;

namespace Reptile
{
	public class EnemyMeleeWeapon : MonoBehaviour
	{
		public enum AttackState
		{
			PRE_ATTACK = 0,
			ATTACK = 1,
			RECOVER = 2,
			NONE = 3
		}

		[SerializeField]
		private string weaponName;

		[SerializeField]
		private float attackInterval;

		[SerializeField]
		private float preAttackDuration;

		[SerializeField]
		private float attackDuration;

		[SerializeField]
		private float attackRecovery;

		[SerializeField]
		public GameObject hitbox;

		[SerializeField]
		public GameObject[] additionalHitBoxes;

		[SerializeField]
		private GameObject visualRoot;

		[SerializeField]
		private float minAttackRange;

		[SerializeField]
		private float maxAttackRange;

		[SerializeField]
		private AudioSource audioSource;

		protected const SfxCollectionID collectionID = SfxCollectionID.CombatSfx;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID attackAudioClipID = AudioClipID.NONE;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID attackLandedAudioClipID = AudioClipID.NONE;

		private AttackState state = AttackState.NONE;

		private bool canAttack;

		private float timebetweenAttacks;

		private float remainingPreAttackDuration;

		private float remainingAttackDuration;

		private float remainingRecoverDuration;

		private bool hitLanded;

		private bool equipped;

		private AudioManager audioManager;

		private EnemyPositioningInfo positioningInfo;

		private bool attackCancelled;

		public AttackState State => state;

		public bool CanAttack
		{
			get
			{
				if (state == AttackState.NONE && timebetweenAttacks <= 0f)
				{
					return equipped;
				}
				return false;
			}
		}

		public bool Attacking
		{
			get
			{
				if (state != AttackState.PRE_ATTACK && state != AttackState.ATTACK)
				{
					return state == AttackState.RECOVER;
				}
				return true;
			}
		}

		public bool HitLanded => hitLanded;

		public bool Equipped => equipped;

		public float MaxAttackRange => maxAttackRange;

		public float MinAttackRange => minAttackRange;

		public float DesiredAttackRange => (minAttackRange + maxAttackRange) / 2f;

		public GameObject VisualRoot => visualRoot;

		public void Init(BasicEnemy enemy, EnemyPositioningInfo positioning)
		{
			audioManager = Core.Instance.AudioManager;
			hitbox.SetActive(value: false);
			for (int i = 0; i < additionalHitBoxes.Length; i++)
			{
				if (additionalHitBoxes[i] != null)
				{
					additionalHitBoxes[i].SetActive(value: false);
				}
			}
			positioningInfo = positioning;
			enemy.OnUpdateActive.AddListener(UpdateWeapon);
		}

		public void Equip(bool equip)
		{
			equipped = equip;
			if (visualRoot != null)
			{
				visualRoot.SetActive(equip);
			}
		}

		private void SetState(AttackState newState)
		{
			ExitState(state);
			state = newState;
			if (state == AttackState.ATTACK && audioSource != null && attackAudioClipID != AudioClipID.NONE)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, attackAudioClipID, audioSource);
			}
			switch (state)
			{
			case AttackState.NONE:
				timebetweenAttacks = attackInterval;
				attackCancelled = false;
				break;
			case AttackState.PRE_ATTACK:
				remainingPreAttackDuration = preAttackDuration;
				break;
			case AttackState.ATTACK:
				ToggleHitboxes(targetState: true);
				remainingAttackDuration = attackDuration;
				break;
			case AttackState.RECOVER:
				remainingRecoverDuration = attackRecovery;
				break;
			}
		}

		public void UpdateWeapon()
		{
			switch (state)
			{
			case AttackState.NONE:
				if (timebetweenAttacks > 0f)
				{
					timebetweenAttacks -= Core.dt;
				}
				break;
			case AttackState.PRE_ATTACK:
				if (remainingPreAttackDuration > 0f)
				{
					remainingPreAttackDuration -= Core.dt;
				}
				if (attackCancelled)
				{
					SetState(AttackState.NONE);
					return;
				}
				if (remainingPreAttackDuration <= 0f)
				{
					SetState(AttackState.ATTACK);
				}
				break;
			case AttackState.ATTACK:
				if (remainingAttackDuration > 0f)
				{
					remainingAttackDuration -= Core.dt;
				}
				if (attackCancelled)
				{
					SetState(AttackState.NONE);
					return;
				}
				if (remainingAttackDuration <= 0f)
				{
					SetState(AttackState.RECOVER);
				}
				break;
			case AttackState.RECOVER:
				if (remainingRecoverDuration > 0f)
				{
					remainingRecoverDuration -= Core.dt;
				}
				if (attackCancelled)
				{
					SetState(AttackState.NONE);
					return;
				}
				if (remainingRecoverDuration <= 0f)
				{
					SetState(AttackState.NONE);
				}
				break;
			}
			canAttack = state == AttackState.NONE && timebetweenAttacks <= 0f && equipped;
		}

		private void ExitState(AttackState exitState)
		{
			switch (exitState)
			{
			case AttackState.ATTACK:
				ToggleHitboxes(targetState: false);
				break;
			case AttackState.RECOVER:
				hitLanded = false;
				break;
			case AttackState.PRE_ATTACK:
			case AttackState.NONE:
				break;
			}
		}

		private void ToggleHitboxes(bool targetState)
		{
			hitbox.SetActive(targetState);
			for (int i = 0; i < additionalHitBoxes.Length; i++)
			{
				if (additionalHitBoxes[i] != null)
				{
					additionalHitBoxes[i].SetActive(targetState);
				}
			}
		}

		public void Attack()
		{
			if (CanAttack)
			{
				SetState(AttackState.PRE_ATTACK);
			}
		}

		public void CancelAttack()
		{
			if (state != AttackState.NONE)
			{
				attackCancelled = true;
				hitLanded = false;
				ToggleHitboxes(targetState: false);
			}
		}

		public bool IsInRangeFlat()
		{
			if (positioningInfo.SqrFlatDistToPlayer < MaxAttackRange * MaxAttackRange)
			{
				return positioningInfo.SqrFlatDistToPlayer > MinAttackRange * MinAttackRange;
			}
			return false;
		}

		public bool IsInRange()
		{
			if (positioningInfo.SqrDistToPlayer < MaxAttackRange * MaxAttackRange)
			{
				return positioningInfo.SqrDistToPlayer > MinAttackRange * MinAttackRange;
			}
			return false;
		}

		public void SetHitLanded()
		{
			if (audioSource != null && attackLandedAudioClipID != AudioClipID.NONE)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, attackLandedAudioClipID, audioSource);
			}
			hitLanded = true;
			hitbox.SetActive(value: false);
			for (int i = 0; i < additionalHitBoxes.Length; i++)
			{
				if (additionalHitBoxes[i] != null)
				{
					additionalHitBoxes[i].SetActive(value: false);
				}
			}
		}
	}
}
